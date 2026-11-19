using RoomHub.Api.Common;

namespace RoomHub.Api.Domain.Rooms;

public sealed class Room
{
    private Room() { } // for EF Core later

    private Room(Guid id, string name, int capacity)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Capacity { get; private set; }

    // R1: a room has a non-empty name and a strictly positive capacity.
    public static Result<Room> Create(string? name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            return RoomErrors.NameRequired;

        if (capacity <= 0)
            return RoomErrors.InvalidCapacity;

        return new Room(Guid.NewGuid(), name.Trim(), capacity);
    }

    public const int LargeRoomThreshold = 10;

    public bool CanHost(int attendees) => attendees <= Capacity;

    // R6 + R10: large rooms (capacity > 10) have a 2-hour limit, the others 4 hours.
    public TimeSpan MaxBookingDuration =>
        Capacity > LargeRoomThreshold ? TimeSpan.FromHours(2) : TimeSpan.FromHours(4);
}
