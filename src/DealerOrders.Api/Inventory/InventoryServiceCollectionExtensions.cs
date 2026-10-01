namespace DealerOrders.Api.Inventory;

public static class InventoryServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryClient(this IServiceCollection services, InventoryOptions options)
    {
        if (string.Equals(options.Mode, "Http", StringComparison.OrdinalIgnoreCase))
        {
            if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUrl))
            {
                throw new InvalidOperationException("Inventory:BaseUrl must be an absolute URL when Inventory:Mode is Http.");
            }

            return services.AddInventoryHttpClient(baseUrl, TimeSpan.FromSeconds(options.TimeoutSeconds));
        }

        if (!string.Equals(options.Mode, "Simulated", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Inventory:Mode must be Simulated or Http.");
        }

        services.AddSingleton<SimulatedInventoryClient>();
        services.AddSingleton<IInventoryClient>(sp => sp.GetRequiredService<SimulatedInventoryClient>());
        return services;
    }

    public static IServiceCollection AddInventoryHttpClient(this IServiceCollection services, Uri baseUrl, TimeSpan timeout)
    {
        services.AddHttpClient<IInventoryClient, InventoryHttpClient>(client =>
        {
            client.BaseAddress = baseUrl;
            client.Timeout = timeout;
        });
        return services;
    }
}

