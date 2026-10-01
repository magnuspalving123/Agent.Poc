using System.Collections.Concurrent;

namespace DealerOrders.Api.Inventory;

/// <summary>Deterministic in-process stand-in for the inventory system. Always accepts and records updates.</summary>
public sealed class SimulatedInventoryClient : IInventoryClient
{
    private readonly ConcurrentQueue<InventoryStockUpdate> _received = new();

    public IReadOnlyCollection<InventoryStockUpdate> Received => _received.ToArray();

    public Task SendStockUpdateAsync(InventoryStockUpdate update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _received.Enqueue(update);
        return Task.CompletedTask;
    }
}

