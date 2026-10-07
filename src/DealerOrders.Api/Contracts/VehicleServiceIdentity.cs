using System.Text.Json.Serialization;

namespace DealerOrders.Api.Contracts;

/// <summary>Isolated mock vehicle service identity JSON contract.</summary>
public sealed record VehicleServiceIdentity
{
    [JsonPropertyName("vehicleIdService")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? VehicleIdService { get; init; }
}
