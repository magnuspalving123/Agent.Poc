using DealerOrders.Api.Domain;
using DealerOrders.Api.Inventory;
using DealerOrders.Api.Services;
using DealerOrders.Api.Storage;
using Microsoft.Extensions.Logging.Abstractions;

namespace DealerOrders.Api.Tests.Unit;

public class OrderUpdateServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 2, 1, 8, 0, 0, TimeSpan.Zero);

    private readonly InMemoryOrderRepository _repository = new();
    private readonly FakeInventoryClient _inventory = new();
    private readonly OrderUpdateService _service;

    public OrderUpdateServiceTests()
    {
        _service = new OrderUpdateService(_repository, _inventory, new FixedTimeProvider(Now), NullLogger<OrderUpdateService>.Instance);
    }

    [Fact]
    public async Task Valid_update_notifies_inventory_and_stores_order()
    {
        var result = await _service.ProcessUpdateAsync("ORD-1001", TestData.ValidRequest(status: "Delivered"), CancellationToken.None);

        Assert.Equal(OrderUpdateOutcome.Processed, result.Outcome);
        Assert.Equal(Now, result.Order!.ProcessedAt);
        var update = Assert.Single(_inventory.Received);
        Assert.Equal(StockStatus.InStock, update.StockStatus);
        var stored = await _repository.GetAsync("ORD-1001", CancellationToken.None);
        Assert.Equal(OrderStatus.Delivered, stored!.Status);
    }

    [Fact]
    public async Task Invalid_update_does_not_call_inventory_or_store()
    {
        var result = await _service.ProcessUpdateAsync("ORD-1001", TestData.ValidRequest(status: "Shipped"), CancellationToken.None);

        Assert.Equal(OrderUpdateOutcome.Invalid, result.Outcome);
        Assert.Contains("status", result.Errors!.Keys);
        Assert.Empty(_inventory.Received);
        Assert.Null(await _repository.GetAsync("ORD-1001", CancellationToken.None));
    }

    [Fact]
    public async Task Inventory_failure_is_reported_and_order_is_not_stored()
    {
        _inventory.ExceptionToThrow = new InventoryUnavailableException("down");

        var result = await _service.ProcessUpdateAsync("ORD-1001", TestData.ValidRequest(), CancellationToken.None);

        Assert.Equal(OrderUpdateOutcome.InventoryUnavailable, result.Outcome);
        Assert.Null(await _repository.GetAsync("ORD-1001", CancellationToken.None));
    }

    [Fact]
    public async Task Inventory_failure_keeps_previously_stored_order()
    {
        await _service.ProcessUpdateAsync("ORD-1001", TestData.ValidRequest(status: "Created"), CancellationToken.None);
        _inventory.ExceptionToThrow = new InventoryUnavailableException("down");

        await _service.ProcessUpdateAsync("ORD-1001", TestData.ValidRequest(status: "Delivered"), CancellationToken.None);

        var stored = await _repository.GetAsync("ORD-1001", CancellationToken.None);
        Assert.Equal(OrderStatus.Created, stored!.Status);
    }

    [Fact]
    public async Task Later_update_replaces_stored_order()
    {
        await _service.ProcessUpdateAsync("ORD-1001", TestData.ValidRequest(status: "Created"), CancellationToken.None);
        await _service.ProcessUpdateAsync("ORD-1001", TestData.ValidRequest(status: "Cancelled"), CancellationToken.None);

        var stored = await _service.GetOrderAsync("ORD-1001", CancellationToken.None);
        Assert.Equal(OrderStatus.Cancelled, stored!.Status);
        Assert.Equal(2, _inventory.Received.Count);
    }

    [Fact]
    public async Task Cancellation_is_propagated_not_reported_as_failure()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _service.ProcessUpdateAsync("ORD-1001", TestData.ValidRequest(), cts.Token));
        Assert.Null(await _repository.GetAsync("ORD-1001", CancellationToken.None));
    }

    [Fact]
    public async Task Unknown_order_returns_null()
    {
        Assert.Null(await _service.GetOrderAsync("ORD-9999", CancellationToken.None));
    }
}

