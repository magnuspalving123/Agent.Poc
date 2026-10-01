using DealerOrders.Api.Domain;
using DealerOrders.Api.Mapping;

namespace DealerOrders.Api.Tests.Unit;

public class OrderMapperTests
{
    private static readonly DateTimeOffset ProcessedAt = new(2026, 2, 1, 8, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(OrderStatus.Created, StockStatus.Reserved)]
    [InlineData(OrderStatus.Confirmed, StockStatus.Reserved)]
    [InlineData(OrderStatus.Delivered, StockStatus.InStock)]
    [InlineData(OrderStatus.Cancelled, StockStatus.Released)]
    public void Order_status_maps_to_stock_status(OrderStatus status, StockStatus expected)
    {
        Assert.Equal(expected, OrderMapper.ToStockStatus(status));
    }

    [Fact]
    public void Request_is_mapped_to_domain_model()
    {
        var request = TestData.ValidRequest(status: "delivered") with { DealerId = " DLR-001 " };

        var order = OrderMapper.ToDomain(request, ProcessedAt);

        Assert.Equal("ORD-1001", order.OrderId);
        Assert.Equal("DLR-001", order.DealerId);
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Equal(new Vehicle(TestData.ValidVin, "Synth Model A", 2025), order.Vehicle);
        Assert.Equal(request.UpdatedAt, order.UpdatedAt);
        Assert.Equal(ProcessedAt, order.ProcessedAt);
    }

    [Fact]
    public void Inventory_update_carries_vin_dealer_stock_status_and_order_reference()
    {
        var order = OrderMapper.ToDomain(TestData.ValidRequest(status: "Delivered"), ProcessedAt);

        var update = OrderMapper.ToInventoryUpdate(order);

        Assert.Equal(TestData.ValidVin, update.Vin);
        Assert.Equal("DLR-001", update.DealerId);
        Assert.Equal(StockStatus.InStock, update.StockStatus);
        Assert.Equal("ORD-1001", update.OrderReference);
    }
}

