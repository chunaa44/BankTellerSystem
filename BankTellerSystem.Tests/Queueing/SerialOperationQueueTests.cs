using BankTellerSystem.Server.Queueing;

namespace BankTellerSystem.Tests.Queueing;

[TestClass]
public class SerialOperationQueueTests
{
    [TestMethod]
    public async Task Enqueue_RunsOperationsInOrder()
    {
        var queue = new SerialOperationQueue();
        var results = new List<int>();

        // First operation deliberately delays. If operations ran concurrently,
        // the second one would finish first and results would be [2, 1].
        var first = queue.Enqueue(async () =>
        {
            await Task.Delay(100);
            results.Add(1);
        });
        var second = queue.Enqueue(async () =>
        {
            await Task.CompletedTask;
            results.Add(2);
        });

        await Task.WhenAll(first, second);

        CollectionAssert.AreEqual(new[] { 1, 2 }, results);
        await queue.DisposeAsync();
    }

    [TestMethod]
    public async Task Enqueue_ReturnsOperationResult()
    {
        var queue = new SerialOperationQueue();

        var result = await queue.Enqueue(async () =>
        {
            await Task.Delay(10);
            return 42;
        });

        Assert.AreEqual(42, result);
        await queue.DisposeAsync();
    }

    [TestMethod]
    public async Task Enqueue_PropagatesExceptionWithoutStoppingQueue()
    {
        var queue = new SerialOperationQueue();

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => queue.Enqueue(() => throw new InvalidOperationException("boom")));

        // The worker loop must still be running after a failed operation.
        var next = await queue.Enqueue(async () =>
        {
            await Task.CompletedTask;
            return "ok";
        });

        Assert.AreEqual("ok", next);
        await queue.DisposeAsync();
    }
}