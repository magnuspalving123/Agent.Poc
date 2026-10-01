namespace DealerOrders.Api.Contracts;

/// <summary>Synthetic inbound contract for a dealer order update.</summary>
public sealed record OrderUpdateRequest(
    string? OrderId,
    string? DealerId,
    string? Status,
    VehicleDto? Vehicle,
    DateTimeOffset? UpdatedAt);

public sealed record VehicleDto(string? Vin, string? Model, int? ModelYear);

