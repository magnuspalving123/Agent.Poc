using DealerOrders.Api.Inventory;

namespace DealerOrders.Api.Tests;

internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

internal sealed class FakeInventoryClient : IInventoryClient
{
    public List<InventoryStockUpdate> Received { get; } = [];

    public Exception? ExceptionToThrow { get; set; }

    public Task SendStockUpdateAsync(InventoryStockUpdate update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        Received.Add(update);
        return Task.CompletedTask;
    }
}

/// <summary>Stands in for the external inventory system at the HTTP boundary.</summary>
internal sealed class StubInventoryHandler : HttpMessageHandler
{
    public List<(HttpMethod Method, Uri? Uri, string Body)> Requests { get; } = [];

    public Func<HttpRequestMessage, HttpResponseMessage> Respond { get; set; } =
        _ => new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add((request.Method, request.RequestUri, body));
        return Respond(request);
    }
}

