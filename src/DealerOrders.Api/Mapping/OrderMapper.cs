using DealerOrders.Api.Contracts;
using DealerOrders.Api.Domain;
using DealerOrders.Api.Inventory;
using DealerOrders.Api.Validation;

namespace DealerOrders.Api.Mapping;

public static class OrderMapper
{
    /// <summary>Maps a request that has already passed <see cref="OrderUpdateValidator"/>.</summary>
    public static DealerOrder ToDomain(OrderUpdateRequest request, DateTimeOffset processedAt)
    {
        if (!OrderStatusParser.TryParse(request.Status, out var status))
        {
            throw new ArgumentException("Status is not valid.", nameof(request));
        }

        var vehicle = request.Vehicle!;
        return new DealerOrder(
            request.OrderId!,
            request.DealerId!.Trim(),
            status,
            new Vehicle(vehicle.Vin!, vehicle.Model!.Trim(), vehicle.ModelYear!.Value),
            request.UpdatedAt!.Value,
            processedAt);
    }

    public static StockStatus ToStockStatus(OrderStatus status) => status switch
    {
        OrderStatus.Created => StockStatus.Reserved,
        OrderStatus.Confirmed => StockStatus.Reserved,
        OrderStatus.Delivered => StockStatus.InStock,
        OrderStatus.Cancelled => StockStatus.Released,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown order status.")
    };

    public static InventoryStockUpdate ToInventoryUpdate(DealerOrder order) =>
        new(order.Vehicle.Vin, order.DealerId, ToStockStatus(order.Status), order.OrderId);

    public static OrderResponse ToResponse(DealerOrder order) =>
        new(
            order.OrderId,
            order.DealerId,
            order.Status,
            new VehicleResponse(order.Vehicle.Vin, order.Vehicle.Model, order.Vehicle.ModelYear),
            order.UpdatedAt,
            order.ProcessedAt);
}

