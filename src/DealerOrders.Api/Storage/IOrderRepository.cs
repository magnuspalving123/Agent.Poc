using DealerOrders.Api.Domain;

namespace DealerOrders.Api.Storage;

public interface IOrderRepository
{
    Task<DealerOrder?> GetAsync(string orderId, CancellationToken cancellationToken);

    /// <summary>Inserts or replaces the stored order (last write wins).</summary>
    Task SaveAsync(DealerOrder order, CancellationToken cancellationToken);
}

