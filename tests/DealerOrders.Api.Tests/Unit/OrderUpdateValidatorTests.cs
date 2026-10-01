using DealerOrders.Api.Contracts;
using DealerOrders.Api.Validation;

namespace DealerOrders.Api.Tests.Unit;

public class OrderUpdateValidatorTests
{
    [Fact]
    public void Valid_request_has_no_errors()
    {
        Assert.Empty(OrderUpdateValidator.Validate("ORD-1001", TestData.ValidRequest()));
    }

    [Theory]
    [InlineData("confirmed")]
    [InlineData("DELIVERED")]
    [InlineData(" Cancelled ")]
    public void Status_is_case_insensitive(string status)
    {
        Assert.Empty(OrderUpdateValidator.Validate("ORD-1001", TestData.ValidRequest(status: status)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Shipped")]
    [InlineData("1")]
    public void Unknown_or_numeric_status_is_rejected(string? status)
    {
        var request = TestData.ValidRequest() with { Status = status };

        Assert.Contains("status", OrderUpdateValidator.Validate("ORD-1001", request).Keys);
    }

    [Fact]
    public void Missing_body_is_rejected()
    {
        Assert.Contains("body", OrderUpdateValidator.Validate("ORD-1001", null).Keys);
    }

    [Fact]
    public void Body_order_id_must_match_route()
    {
        var errors = OrderUpdateValidator.Validate("ORD-1001", TestData.ValidRequest(orderId: "ORD-2002"));

        Assert.Contains("orderId", errors.Keys);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("ord-1001")]
    [InlineData("ORD 1001")]
    public void Malformed_route_order_id_is_rejected(string orderId)
    {
        var errors = OrderUpdateValidator.Validate(orderId, TestData.ValidRequest(orderId: orderId));

        Assert.Contains("orderId", errors.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("TOOSHORT")]
    [InlineData("TESTVEH000000000I")]
    [InlineData("testveh0000000001")]
    public void Invalid_vin_is_rejected(string? vin)
    {
        var request = TestData.ValidRequest() with { Vehicle = new VehicleDto(vin, "Synth Model A", 2025) };

        Assert.Contains("vehicle.vin", OrderUpdateValidator.Validate("ORD-1001", request).Keys);
    }

    [Theory]
    [InlineData(1989)]
    [InlineData(2101)]
    public void Model_year_out_of_range_is_rejected(int year)
    {
        var request = TestData.ValidRequest() with { Vehicle = new VehicleDto(TestData.ValidVin, "Synth Model A", year) };

        Assert.Contains("vehicle.modelYear", OrderUpdateValidator.Validate("ORD-1001", request).Keys);
    }

    [Fact]
    public void Missing_fields_are_all_reported()
    {
        var errors = OrderUpdateValidator.Validate("ORD-1001", new OrderUpdateRequest("ORD-1001", " ", "Created", null, null));

        Assert.Contains("dealerId", errors.Keys);
        Assert.Contains("vehicle", errors.Keys);
        Assert.Contains("updatedAt", errors.Keys);
    }
}

