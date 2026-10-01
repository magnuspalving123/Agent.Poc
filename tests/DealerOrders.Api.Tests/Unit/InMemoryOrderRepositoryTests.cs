using DealerOrders.Api.Mapping;
using DealerOrders.Api.Storage;

namespace DealerOrders.Api.Tests.Unit;

public class InMemoryOrderRepositoryTests
{
    [Fact]
    public async Task Saved_order_can_be_read_back()
    {
        var repository = new InMemoryOrderRepository();
        var order = OrderMapper.ToDomain(TestData.ValidRequest(), DateTimeOffset.UnixEpoch);

        await repository.SaveAsync(order, CancellationToken.None);

        Assert.Equal(order, await repository.GetAsync("ORD-1001", CancellationToken.None));
    }

    [Fact]
    public async Task Unknown_order_is_null_and_ids_are_case_sensitive()
    {
        var repository = new InMemoryOrderRepository();
        await repository.SaveAsync(OrderMapper.ToDomain(TestData.ValidRequest(), DateTimeOffset.UnixEpoch), CancellationToken.None);

        Assert.Null(await repository.GetAsync("ord-1001", CancellationToken.None));
    }
}

