using DealerOrders.Api.Contracts;

namespace DealerOrders.Api.Tests;

internal static class TestData
{
    public const string ValidVin = "TESTVEH0000000001";

    public static OrderUpdateRequest ValidRequest(
        string orderId = "ORD-1001",
        string status = "Confirmed",
        string vin = ValidVin) =>
        new(
            orderId,
            "DLR-001",
            status,
            new VehicleDto(vin, "Synth Model A", 2025),
            new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero));
}

