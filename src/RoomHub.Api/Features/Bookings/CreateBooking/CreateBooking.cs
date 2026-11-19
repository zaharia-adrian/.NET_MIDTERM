using RoomHub.Api.Common;
using RoomHub.Api.Domain;
using RoomHub.Api.Domain.Bookings;
using RoomHub.Api.Domain.Rooms;

namespace RoomHub.Api.Features.Bookings.CreateBooking;

public sealed record CreateBookingRequest(string? BookedBy, DateTime StartUtc, DateTime EndUtc, int Attendees);

public sealed record CreateBookingCommand(Guid RoomId, string? BookedBy, DateTime StartUtc, DateTime EndUtc, int Attendees);

public sealed record CreateBookingResponse(Guid Id, Guid RoomId, string Status);

public sealed class CreateBookingHandler(
    IRoomRepository rooms,
    IBookingRepository bookings,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    BookingWriteGate writeGate)
    : ICommandHandler<CreateBookingCommand, CreateBookingResponse>
{
    public async Task<Result<CreateBookingResponse>> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        // 404 before any validation.
        var room = await rooms.GetByIdAsync(command.RoomId, cancellationToken);
        if (room is null)
            return RoomErrors.NotFound;

        // Load + check (R7, R9) + save must happen as one step, otherwise parallel requests can both pass.
        using var _ = await writeGate.EnterAsync(cancellationToken);

        var existing = await bookings.GetActiveForRoomAsync(room.Id, cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        // R9 needs the participant's bookings on the same UTC day (the domain decides; the handler only loads data).
        IReadOnlyList<Booking> sameDay = string.IsNullOrWhiteSpace(command.BookedBy)
            ? []
            : await bookings.GetActiveForParticipantOnDayAsync(
                command.BookedBy.Trim(), DateOnly.FromDateTime(command.StartUtc), cancellationToken);

        var result = Booking.Create(
            room, command.BookedBy, command.StartUtc, command.EndUtc, command.Attendees, now, existing, sameDay);
        if (result.IsFailure)
            return result.Error;

        var booking = result.Value;
        bookings.Add(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateBookingResponse(booking.Id, booking.RoomId, booking.Status.ToString());
    }
}
