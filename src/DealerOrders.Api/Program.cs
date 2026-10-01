using System.Text.Json.Serialization;
using DealerOrders.Api.Contracts;
using DealerOrders.Api.Inventory;
using DealerOrders.Api.Mapping;
using DealerOrders.Api.Services;
using DealerOrders.Api.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton<OrderUpdateService>();
builder.Services.AddInventoryClient(
    builder.Configuration.GetSection(InventoryOptions.SectionName).Get<InventoryOptions>() ?? new InventoryOptions());

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPut("/api/orders/{orderId}", async (
    string orderId,
    OrderUpdateRequest? request,
    OrderUpdateService service,
    CancellationToken cancellationToken) =>
{
    var result = await service.ProcessUpdateAsync(orderId, request, cancellationToken);
    return result.Outcome switch
    {
        OrderUpdateOutcome.Processed => Results.Ok(OrderMapper.ToResponse(result.Order!)),
        OrderUpdateOutcome.Invalid => Results.ValidationProblem(result.Errors!.ToDictionary(e => e.Key, e => e.Value)),
        _ => Results.Problem(
            statusCode: StatusCodes.Status502BadGateway,
            title: "Inventory system unavailable",
            detail: "The order update was not stored because the inventory system could not be updated.")
    };
});

app.MapGet("/api/orders/{orderId}", async (
    string orderId,
    OrderUpdateService service,
    CancellationToken cancellationToken) =>
{
    var order = await service.GetOrderAsync(orderId, cancellationToken);
    return order is null
        ? Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Order not found")
        : Results.Ok(OrderMapper.ToResponse(order));
});

// Local diagnostics: shows what the simulated inventory system has received.
var simulator = app.Services.GetService<SimulatedInventoryClient>();
if (simulator is not null)
{
    app.MapGet("/simulated-inventory/stock-updates", () => Results.Ok(simulator.Received));
}

app.Run();

public partial class Program;

