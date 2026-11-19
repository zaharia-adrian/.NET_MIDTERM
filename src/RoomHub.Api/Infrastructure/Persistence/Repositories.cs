using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Domain.Bookings;
using RoomHub.Api.Domain.Rooms;

namespace RoomHub.Api.Infrastructure.Persistence;

// Repositories implement the interfaces declared in the domain. They never call SaveChanges:
// that is the job of the Unit of Work (RoomHubDbContext), called by the handler.

public sealed class RoomRepository(RoomHubDbContext db) : IRoomRepository
{
    public Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Rooms.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public void Add(Room room) => db.Rooms.Add(room);
}

public sealed class BookingRepository(RoomHubDbContext db) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetActiveForRoomAsync(Guid roomId, CancellationToken cancellationToken = default) =>
        await db.Bookings
            .Where(b => b.RoomId == roomId && b.Status == BookingStatus.Active)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetActiveForParticipantOnDayAsync(
        string bookedBy, DateOnly dayUtc, CancellationToken cancellationToken = default)
    {
        var dayStart = dayUtc.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);

        return await db.Bookings
            .Where(b => b.BookedBy == bookedBy
                        && b.Status == BookingStatus.Active
                        && b.StartUtc >= dayStart
                        && b.StartUtc < dayEnd)
            .ToListAsync(cancellationToken);
    }

    public void Add(Booking booking) => db.Bookings.Add(booking);
}
