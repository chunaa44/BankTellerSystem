using System.Threading.Channels;

namespace BankTellerSystem.Server.Queueing;

// Runs async operations strictly one-at-a-time, in the order they were enqueued.
// Used to serialize things like "call next ticket" or "transfer money" so two
// requests arriving at the same moment can't be processed concurrently.
public class SerialOperationQueue : IAsyncDisposable
{
    private readonly Channel<Func<Task>> _channel = Channel.CreateUnbounded<Func<Task>>();
    private readonly Task _worker;

    public SerialOperationQueue()
    {
        // Single background loop drains the channel - this is what guarantees
        // "one at a time", since only one operation is ever awaited at once.
        _worker = Task.Run(ProcessQueueAsync);
    }

    // Queue an operation with no return value. The returned Task completes
    // once the operation has actually run (not just once it's enqueued).
    public Task Enqueue(Func<Task> operation)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        _channel.Writer.TryWrite(async () =>
        {
            try
            {
                await operation();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    // Same as above, but for operations that produce a result (e.g. the ticket
    // number that was just called).
    public Task<T> Enqueue<T>(Func<Task<T>> operation)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        _channel.Writer.TryWrite(async () =>
        {
            try
            {
                var result = await operation();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    private async Task ProcessQueueAsync()
    {
        // ReadAllAsync yields items in write order and waits for new ones -
        // this loop is the entire "queue" behavior.
        await foreach (var operation in _channel.Reader.ReadAllAsync())
        {
            await operation();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.Complete();
        await _worker;
    }
}