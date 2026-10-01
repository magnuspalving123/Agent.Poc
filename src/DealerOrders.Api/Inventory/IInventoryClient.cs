namespace DealerOrders.Api.Inventory;

public interface IInventoryClient
{
    /// <exception cref="InventoryUnavailableException">The inventory system rejected the update or could not be reached.</exception>
    Task SendStockUpdateAsync(InventoryStockUpdate update, CancellationToken cancellationToken);
}

