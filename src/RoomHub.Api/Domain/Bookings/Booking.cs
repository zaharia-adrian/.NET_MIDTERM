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
    public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(4);

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
        IEnumerable<Booking> existingRoomBookings)
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

        // R6: exactly 4 hours is allowed.
        if (endUtc - startUtc > MaxDuration)
            return BookingErrors.TooLong;

        // R7: only active bookings of this room count; touching intervals do not overlap.
        if (existingRoomBookings.Any(b => b.RoomId == room.Id && b.OverlapsWith(startUtc, endUtc)))
            return BookingErrors.RoomOccupied;

        return new Booking(Guid.NewGuid(), room.Id, bookedBy.Trim(), startUtc, endUtc, attendees);
    }

    // Half-open intervals [Start, End): ending exactly when another starts is not an overlap.
    public bool OverlapsWith(DateTime startUtc, DateTime endUtc) =>
        IsActive && StartUtc < endUtc && startUtc < EndUtc;

    // R8
    public Result Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            return BookingErrors.AlreadyCancelled;

        Status = BookingStatus.Cancelled;
        return Result.Success();
    }
}
