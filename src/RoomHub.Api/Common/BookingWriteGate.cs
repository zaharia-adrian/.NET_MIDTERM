namespace RoomHub.Api.Common;

// R7 / R8 / R9 are "check, then write" rules. Without this gate two parallel requests could both pass the
// check (e.g. both see a free room) and both save. Registered as a singleton; one booking write at a time.
public sealed class BookingWriteGate
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<IDisposable> EnterAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        return new Releaser(_semaphore);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
                semaphore.Release();
        }
    }
}
