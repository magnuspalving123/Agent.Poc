using System.Text.Json.Serialization;

namespace DealerOrders.Api.Contracts;

public sealed record VehicleServiceIdentity
{
    [JsonPropertyName("vehicleIdService")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? VehicleIdService { get; init; }
}
