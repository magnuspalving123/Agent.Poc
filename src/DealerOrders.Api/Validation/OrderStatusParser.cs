using DealerOrders.Api.Domain;

namespace DealerOrders.Api.Validation;

public static class OrderStatusParser
{
    public static IReadOnlyList<string> Names { get; } = Enum.GetNames<OrderStatus>();

    /// <summary>Case-insensitive match on status names. Numeric values are rejected.</summary>
    public static bool TryParse(string? value, out OrderStatus status)
    {
        status = default;
        if (string.IsNullOrWhiteSpace(value) || char.IsDigit(value.Trim()[0]))
        {
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out status) && Enum.IsDefined(status);
    }
}

