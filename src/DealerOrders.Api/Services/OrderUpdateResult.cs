using DealerOrders.Api.Domain;

namespace DealerOrders.Api.Services;

public enum OrderUpdateOutcome
{
    Processed,
    Invalid,
    InventoryUnavailable
}

public sealed record OrderUpdateResult(
    OrderUpdateOutcome Outcome,
    DealerOrder? Order = null,
    IReadOnlyDictionary<string, string[]>? Errors = null);

