using System.Collections.Concurrent;
using DealerOrders.Api.Domain;

namespace DealerOrders.Api.Storage;

/// <summary>Process-local storage. All data is lost when the host restarts.</summary>
public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<string, DealerOrder> _orders = new(StringComparer.Ordinal);

    public Task<DealerOrder?> GetAsync(string orderId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _orders.TryGetValue(orderId, out var order);
        return Task.FromResult(order);
    }

    public Task SaveAsync(DealerOrder order, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _orders[order.OrderId] = order;
        return Task.CompletedTask;
    }
}

