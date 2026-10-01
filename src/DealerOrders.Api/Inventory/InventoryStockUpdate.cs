using DealerOrders.Api.Domain;

namespace DealerOrders.Api.Inventory;

/// <summary>Synthetic outbound contract sent to the external inventory system.</summary>
public sealed record InventoryStockUpdate(string Vin, string DealerId, StockStatus StockStatus, string OrderReference);

