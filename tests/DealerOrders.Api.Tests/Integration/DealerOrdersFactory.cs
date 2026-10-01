using DealerOrders.Api.Inventory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DealerOrders.Api.Tests.Integration;

/// <summary>Runs the real host in-process with the inventory HTTP boundary replaced by a stub handler.</summary>
public sealed class DealerOrdersFactory : WebApplicationFactory<Program>
{
    internal StubInventoryHandler Inventory { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IInventoryClient>();
            services.AddInventoryHttpClient(new Uri("http://inventory.test/"), TimeSpan.FromSeconds(5))
                .AddHttpClient<IInventoryClient, InventoryHttpClient>()
                .ConfigurePrimaryHttpMessageHandler(() => Inventory);
        });
    }
}

