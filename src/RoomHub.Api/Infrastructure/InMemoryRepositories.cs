using RoomHub.Api.Domain;
using RoomHub.Api.Domain.Bookings;
using RoomHub.Api.Domain.Rooms;

namespace RoomHub.Api.Infrastructure;

// Temporary in-memory persistence (data is lost on restart). To be replaced by EF Core + SQLite.

public sealed class InMemoryRoomRepository : IRoomRepository
{
    private readonly List<Room> _rooms = new();
    private readonly object _lock = new();

    public Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
            return Task.FromResult(_rooms.FirstOrDefault(r => r.Id == id));
    }

    public void Add(Room room)
    {
        lock (_lock)
            _rooms.Add(room);
    }
}

public sealed class InMemoryBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings = new();
    private readonly object _lock = new();

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
            return Task.FromResult(_bookings.FirstOrDefault(b => b.Id == id));
    }

    public Task<IReadOnlyList<Booking>> GetActiveForRoomAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<Booking> result = _bookings
                .Where(b => b.RoomId == roomId && b.IsActive)
                .ToList();
            return Task.FromResult(result);
        }
    }

    public void Add(Booking booking)
    {
        lock (_lock)
            _bookings.Add(booking);
    }
}

// Objects are held in memory, so there is nothing to flush; EF Core's DbContext will do the real work later.
public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
