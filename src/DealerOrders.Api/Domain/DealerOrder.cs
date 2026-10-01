namespace DealerOrders.Api.Domain;

public sealed record Vehicle(string Vin, string Model, int ModelYear);

/// <summary>Internal order model. <paramref name="UpdatedAt"/> comes from the sender, <paramref name="ProcessedAt"/> from this service.</summary>
public sealed record DealerOrder(
    string OrderId,
    string DealerId,
    OrderStatus Status,
    Vehicle Vehicle,
    DateTimeOffset UpdatedAt,
    DateTimeOffset ProcessedAt);

