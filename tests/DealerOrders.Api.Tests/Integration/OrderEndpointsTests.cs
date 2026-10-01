using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DealerOrders.Api.Tests.Integration;

public class OrderEndpointsTests : IDisposable
{
    private readonly DealerOrdersFactory _factory = new();
    private readonly HttpClient _client;

    public OrderEndpointsTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private const string ValidJson = """
        {
          "orderId": "ORD-1001",
          "dealerId": "DLR-001",
          "status": "Delivered",
          "vehicle": { "vin": "TESTVEH0000000001", "model": "Synth Model A", "modelYear": 2025 },
          "updatedAt": "2026-01-15T10:00:00Z"
        }
        """;

    private static StringContent Json(string json) => new(json, System.Text.Encoding.UTF8, "application/json");

    [Fact]
    public async Task Health_returns_ok()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Valid_update_is_processed_and_can_be_read_back()
    {
        var put = await _client.PutAsync("/api/orders/ORD-1001", Json(ValidJson));

        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        var get = await _client.GetAsync("/api/orders/ORD-1001");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        using var json = JsonDocument.Parse(await get.Content.ReadAsStringAsync());
        Assert.Equal("ORD-1001", json.RootElement.GetProperty("orderId").GetString());
        Assert.Equal("Delivered", json.RootElement.GetProperty("status").GetString());
        Assert.Equal("TESTVEH0000000001", json.RootElement.GetProperty("vehicle").GetProperty("vin").GetString());
        Assert.True(json.RootElement.TryGetProperty("processedAt", out _));
    }

    [Fact]
    public async Task Valid_update_sends_mapped_stock_update_to_inventory_system()
    {
        await _client.PutAsync("/api/orders/ORD-1001", Json(ValidJson));

        var request = Assert.Single(_factory.Inventory.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/v1/stock-updates", request.Uri!.AbsolutePath);
        using var json = JsonDocument.Parse(request.Body);
        Assert.Equal("InStock", json.RootElement.GetProperty("stockStatus").GetString());
        Assert.Equal("TESTVEH0000000001", json.RootElement.GetProperty("vin").GetString());
        Assert.Equal("ORD-1001", json.RootElement.GetProperty("orderReference").GetString());
    }

    [Fact]
    public async Task Second_update_replaces_first()
    {
        await _client.PutAsync("/api/orders/ORD-1001", Json(ValidJson.Replace("Delivered", "Created")));
        await _client.PutAsync("/api/orders/ORD-1001", Json(ValidJson.Replace("Delivered", "Cancelled")));

        var order = await _client.GetFromJsonAsync<JsonElement>("/api/orders/ORD-1001");
        Assert.Equal("Cancelled", order.GetProperty("status").GetString());
        Assert.Equal(2, _factory.Inventory.Requests.Count);
    }

    [Fact]
    public async Task Invalid_status_returns_400_with_field_errors_and_nothing_is_sent_or_stored()
    {
        var put = await _client.PutAsync("/api/orders/ORD-1001", Json(ValidJson.Replace("Delivered", "Shipped")));

        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
        using var json = JsonDocument.Parse(await put.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("errors").TryGetProperty("status", out _));
        Assert.Empty(_factory.Inventory.Requests);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/orders/ORD-1001")).StatusCode);
    }

    [Fact]
    public async Task Route_and_body_order_id_mismatch_returns_400()
    {
        var put = await _client.PutAsync("/api/orders/ORD-2002", Json(ValidJson));

        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
        Assert.Empty(_factory.Inventory.Requests);
    }

    [Fact]
    public async Task Malformed_json_returns_400()
    {
        var put = await _client.PutAsync("/api/orders/ORD-1001", Json("{ not json"));

        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
    }

    [Fact]
    public async Task Missing_body_returns_400()
    {
        var put = await _client.PutAsync("/api/orders/ORD-1001", new ByteArrayContent([]));

        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
    }

    [Fact]
    public async Task Unknown_order_returns_404()
    {
        var get = await _client.GetAsync("/api/orders/ORD-9999");

        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Inventory_failure_returns_502_and_order_is_not_stored()
    {
        _factory.Inventory.Respond = _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

        var put = await _client.PutAsync("/api/orders/ORD-1001", Json(ValidJson));

        Assert.Equal(HttpStatusCode.BadGateway, put.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/orders/ORD-1001")).StatusCode);
    }

    [Fact]
    public async Task Inventory_network_failure_returns_502()
    {
        _factory.Inventory.Respond = _ => throw new HttpRequestException("connection refused");

        var put = await _client.PutAsync("/api/orders/ORD-1001", Json(ValidJson));

        Assert.Equal(HttpStatusCode.BadGateway, put.StatusCode);
    }
}

