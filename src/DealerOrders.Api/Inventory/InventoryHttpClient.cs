using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DealerOrders.Api.Inventory;

/// <summary>Typed HttpClient for the synthetic inventory API (POST v1/stock-updates).</summary>
public sealed class InventoryHttpClient : IInventoryClient
{
    public const string StockUpdatesPath = "v1/stock-updates";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient;

    public InventoryHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendStockUpdateAsync(InventoryStockUpdate update, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(StockUpdatesPath, update, JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InventoryUnavailableException(
                    $"Inventory system responded with status {(int)response.StatusCode}.");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new InventoryUnavailableException("Inventory system could not be reached.", ex);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient timeout: the caller did not cancel.
            throw new InventoryUnavailableException("Inventory system did not respond in time.", ex);
        }
    }
}

