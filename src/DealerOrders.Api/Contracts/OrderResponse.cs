using DealerOrders.Api.Domain;

namespace DealerOrders.Api.Contracts;

public sealed record OrderResponse(
    string OrderId,
    string DealerId,
    OrderStatus Status,
    VehicleResponse Vehicle,
    DateTimeOffset UpdatedAt,
    DateTimeOffset ProcessedAt);

public sealed record VehicleResponse(string Vin, string Model, int ModelYear);

