namespace DealerOrders.Api.Inventory;

public sealed class InventoryOptions
{
    public const string SectionName = "Inventory";

    /// <summary>"Simulated" (default, in-process) or "Http".</summary>
    public string Mode { get; set; } = "Simulated";

    /// <summary>Absolute base URL of the inventory system. Required when Mode is "Http".</summary>
    public string? BaseUrl { get; set; }

    public int TimeoutSeconds { get; set; } = 5;
}

