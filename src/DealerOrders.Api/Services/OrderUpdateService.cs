using DealerOrders.Api.Contracts;
using DealerOrders.Api.Domain;
using DealerOrders.Api.Inventory;
using DealerOrders.Api.Mapping;
using DealerOrders.Api.Storage;
using DealerOrders.Api.Validation;

namespace DealerOrders.Api.Services;

public sealed class OrderUpdateService
{
    private readonly IOrderRepository _repository;
    private readonly IInventoryClient _inventoryClient;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<OrderUpdateService> _logger;

    public OrderUpdateService(
        IOrderRepository repository,
        IInventoryClient inventoryClient,
        TimeProvider timeProvider,
        ILogger<OrderUpdateService> logger)
    {
        _repository = repository;
        _inventoryClient = inventoryClient;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Validates, maps, notifies the inventory system and then stores the order.
    /// The order is only stored when the inventory update succeeded.
    /// </summary>
    public async Task<OrderUpdateResult> ProcessUpdateAsync(
        string routeOrderId,
        OrderUpdateRequest? request,
        CancellationToken cancellationToken)
    {
        var errors = OrderUpdateValidator.Validate(routeOrderId, request);
        if (errors.Count > 0)
        {
            _logger.LogWarning("Order update {OrderId} rejected with {ErrorCount} validation errors", routeOrderId, errors.Count);
            return new OrderUpdateResult(OrderUpdateOutcome.Invalid, Errors: errors);
        }

        var order = OrderMapper.ToDomain(request!, _timeProvider.GetUtcNow());

        try
        {
            await _inventoryClient.SendStockUpdateAsync(OrderMapper.ToInventoryUpdate(order), cancellationToken);
        }
        catch (InventoryUnavailableException ex)
        {
            _logger.LogError(ex, "Inventory update failed for order {OrderId}; order not stored", order.OrderId);
            return new OrderUpdateResult(OrderUpdateOutcome.InventoryUnavailable);
        }

        await _repository.SaveAsync(order, cancellationToken);
        _logger.LogInformation(
            "Order {OrderId} processed with status {Status} for dealer {DealerId}",
            order.OrderId, order.Status, order.DealerId);
        return new OrderUpdateResult(OrderUpdateOutcome.Processed, order);
    }

    public Task<DealerOrder?> GetOrderAsync(string orderId, CancellationToken cancellationToken) =>
        _repository.GetAsync(orderId, cancellationToken);
}

