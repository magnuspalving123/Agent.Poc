using System.Text.RegularExpressions;
using DealerOrders.Api.Contracts;

namespace DealerOrders.Api.Validation;

public static partial class OrderUpdateValidator
{
    private const int MinModelYear = 1990;
    private const int MaxModelYear = 2100;

    [GeneratedRegex("^[A-Z0-9-]{3,32}$")]
    private static partial Regex OrderIdPattern();

    // 17 characters, letters and digits, excluding I, O and Q.
    [GeneratedRegex("^[A-HJ-NPR-Z0-9]{17}$")]
    private static partial Regex VinPattern();

    /// <summary>Returns field name to messages; an empty dictionary means the request is valid.</summary>
    public static IReadOnlyDictionary<string, string[]> Validate(string routeOrderId, OrderUpdateRequest? request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request is null)
        {
            errors["body"] = ["A request body is required."];
            return errors;
        }

        if (string.IsNullOrWhiteSpace(routeOrderId) || !OrderIdPattern().IsMatch(routeOrderId))
        {
            errors["orderId"] = ["Order id must be 3-32 characters: upper-case letters, digits and hyphen."];
        }
        else if (!string.Equals(request.OrderId, routeOrderId, StringComparison.Ordinal))
        {
            errors["orderId"] = ["Order id in the body must match the order id in the route."];
        }

        if (string.IsNullOrWhiteSpace(request.DealerId))
        {
            errors["dealerId"] = ["Dealer id is required."];
        }

        if (!OrderStatusParser.TryParse(request.Status, out _))
        {
            errors["status"] = [$"Status must be one of: {string.Join(", ", OrderStatusParser.Names)}."];
        }

        if (request.UpdatedAt is null || request.UpdatedAt == default(DateTimeOffset))
        {
            errors["updatedAt"] = ["Updated-at timestamp is required."];
        }

        ValidateVehicle(request.Vehicle, errors);
        return errors;
    }

    private static void ValidateVehicle(VehicleDto? vehicle, Dictionary<string, string[]> errors)
    {
        if (vehicle is null)
        {
            errors["vehicle"] = ["Vehicle is required."];
            return;
        }

        if (vehicle.Vin is null || !VinPattern().IsMatch(vehicle.Vin))
        {
            errors["vehicle.vin"] = ["VIN must be 17 upper-case letters or digits (I, O and Q are not allowed)."];
        }

        if (string.IsNullOrWhiteSpace(vehicle.Model))
        {
            errors["vehicle.model"] = ["Model is required."];
        }

        if (vehicle.ModelYear is null or < MinModelYear or > MaxModelYear)
        {
            errors["vehicle.modelYear"] = [$"Model year must be between {MinModelYear} and {MaxModelYear}."];
        }
    }
}

