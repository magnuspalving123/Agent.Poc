using System.Net;
using System.Text.Json;
using DealerOrders.Api.Domain;
using DealerOrders.Api.Inventory;

namespace DealerOrders.Api.Tests.Unit;

public class InventoryHttpClientTests
{
    private static readonly InventoryStockUpdate Update = new(TestData.ValidVin, "DLR-001", StockStatus.InStock, "ORD-1001");

    private static (InventoryHttpClient Client, StubInventoryHandler Handler) Create()
    {
        var handler = new StubInventoryHandler();
        var client = new InventoryHttpClient(new HttpClient(handler) { BaseAddress = new Uri("http://inventory.test/") });
        return (client, handler);
    }

    [Fact]
    public async Task Posts_json_contract_to_stock_updates_endpoint()
    {
        var (client, handler) = Create();

        await client.SendStockUpdateAsync(Update, CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("http://inventory.test/v1/stock-updates", request.Uri!.ToString());
        using var json = JsonDocument.Parse(request.Body);
        Assert.Equal(TestData.ValidVin, json.RootElement.GetProperty("vin").GetString());
        Assert.Equal("DLR-001", json.RootElement.GetProperty("dealerId").GetString());
        Assert.Equal("InStock", json.RootElement.GetProperty("stockStatus").GetString());
        Assert.Equal("ORD-1001", json.RootElement.GetProperty("orderReference").GetString());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Non_success_status_becomes_inventory_unavailable(HttpStatusCode status)
    {
        var (client, handler) = Create();
        handler.Respond = _ => new HttpResponseMessage(status);

        await Assert.ThrowsAsync<InventoryUnavailableException>(() => client.SendStockUpdateAsync(Update, CancellationToken.None));
    }

    [Fact]
    public async Task Network_failure_becomes_inventory_unavailable()
    {
        var (client, handler) = Create();
        handler.Respond = _ => throw new HttpRequestException("connection refused");

        await Assert.ThrowsAsync<InventoryUnavailableException>(() => client.SendStockUpdateAsync(Update, CancellationToken.None));
    }

    [Fact]
    public async Task Timeout_becomes_inventory_unavailable()
    {
        var (client, handler) = Create();
        handler.Respond = _ => throw new TaskCanceledException("timeout");

        await Assert.ThrowsAsync<InventoryUnavailableException>(() => client.SendStockUpdateAsync(Update, CancellationToken.None));
    }

    [Fact]
    public async Task Caller_cancellation_is_not_converted()
    {
        var (client, _) = Create();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.SendStockUpdateAsync(Update, cts.Token));
    }
}

