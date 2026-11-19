using RoomHub.Api.Common;
using RoomHub.Api.Domain.Rooms;

namespace RoomHub.Api.Domain.Bookings;

public enum BookingStatus
{
    Active,
    Cancelled
}

public sealed class Booking
{
    public const int MaxActiveBookingsPerDay = 2;
    public static readonly TimeSpan MinNoticeForCancel = TimeSpan.FromMinutes(60);

    private Booking() { } // for EF Core later

    private Booking(Guid id, Guid roomId, string bookedBy, DateTime startUtc, DateTime endUtc, int attendees)
    {
        Id = id;
        RoomId = roomId;
        BookedBy = bookedBy;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Attendees = attendees;
        Status = BookingStatus.Active;
    }

    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public string BookedBy { get; private set; } = string.Empty;
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public int Attendees { get; private set; }
    public BookingStatus Status { get; private set; }

    public bool IsActive => Status == BookingStatus.Active;

    public static Result<Booking> Create(
        Room room,
        string? bookedBy,
        DateTime startUtc,
        DateTime endUtc,
        int attendees,
        DateTime nowUtc,
        IEnumerable<Booking> existingRoomBookings,
        IEnumerable<Booking> participantBookingsSameDay)
    {
        // R2
        if (string.IsNullOrWhiteSpace(bookedBy))
            return BookingErrors.BookedByRequired;

        // R3
        if (endUtc <= startUtc)
            return BookingErrors.InvalidInterval;

        // R4
        if (attendees <= 0)
            return BookingErrors.InvalidAttendees;

        if (!room.CanHost(attendees))
            return BookingErrors.ExceedsCapacity;

        // R5: "now" is supplied by the caller (from TimeProvider), never DateTime.UtcNow.
        if (startUtc < nowUtc)
            return BookingErrors.StartsInThePast;

        // R6 + R10: the limit depends on the room (4h, or 2h for capacity > 10); the limit itself is allowed.
        var maxDuration = room.MaxBookingDuration;
        if (endUtc - startUtc > maxDuration)
            return BookingErrors.TooLong(maxDuration);

        // R7: only active bookings of this room count; touching intervals do not overlap.
        if (existingRoomBookings.Any(b => b.RoomId == room.Id && b.OverlapsWith(startUtc, endUtc)))
            return BookingErrors.RoomOccupied;

        // R9: at most 2 active bookings per participant per UTC day (day = date of StartUtc), all rooms.
        var participant = bookedBy.Trim();
        var day = DateOnly.FromDateTime(startUtc);
        var activeSameDay = participantBookingsSameDay.Count(b =>
            b.IsActive && b.BookedBy == participant && DateOnly.FromDateTime(b.StartUtc) == day);
        if (activeSameDay >= MaxActiveBookingsPerDay)
            return BookingErrors.DailyLimitReached;

        return new Booking(Guid.NewGuid(), room.Id, participant, startUtc, endUtc, attendees);
    }

    // Half-open intervals [Start, End): ending exactly when another starts is not an overlap.
    public bool OverlapsWith(DateTime startUtc, DateTime endUtc) =>
        IsActive && StartUtc < endUtc && startUtc < EndUtc;

    // R8 + R11
    public Result Cancel(DateTime nowUtc)
    {
        if (Status == BookingStatus.Cancelled)
            return BookingErrors.AlreadyCancelled;

        // R11: must start at least 60 minutes from now (exactly 60 is allowed).
        if (StartUtc - nowUtc < MinNoticeForCancel)
            return BookingErrors.TooLateToCancel;

        Status = BookingStatus.Cancelled;
        return Result.Success();
    }
}
