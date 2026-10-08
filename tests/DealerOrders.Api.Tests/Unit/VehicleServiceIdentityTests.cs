using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using DealerOrders.Api.Contracts;

namespace DealerOrders.Api.Tests.Unit;

public class VehicleServiceIdentityTests
{
    public static TheoryData<string> StringValues => new()
    {
        "",
        " \t\r\n ",
        "mIxEdCaSe",
        "Blåbær 日本語 🚗",
        "ordinary text",
        "VW47586863",
        " not-a-VIN!? ",
        new string('x', 100_000)
    };

    [Fact]
    public void Declaration_is_public_sealed_record_with_one_nullable_string_init_property()
    {
        var type = typeof(VehicleServiceIdentity);
        var property = Assert.Single(type.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.Equal(nameof(VehicleServiceIdentity.VehicleIdService), property.Name);
        Assert.Equal(typeof(string), property.PropertyType);
        Assert.True(property.GetMethod!.IsPublic);
        Assert.True(property.SetMethod!.IsPublic);
        Assert.Contains(typeof(IsExternalInit), property.SetMethod.ReturnParameter.GetRequiredCustomModifiers());
        Assert.Equal(NullabilityState.Nullable, new NullabilityInfoContext().Create(property).ReadState);

        var identity = new VehicleServiceIdentity { VehicleIdService = "VW47586863" };
        var copy = identity with { };
        Assert.NotSame(identity, copy);
        Assert.Equal(identity, copy);
    }

    [Fact]
    public void Json_name_and_null_inclusion_are_declared_on_property()
    {
        var property = typeof(VehicleServiceIdentity).GetProperty(nameof(VehicleServiceIdentity.VehicleIdService))!;
        var name = property.GetCustomAttribute<JsonPropertyNameAttribute>();
        var ignore = property.GetCustomAttribute<JsonIgnoreAttribute>();

        Assert.NotNull(name);
        Assert.Equal("vehicleIdService", name.Name);
        Assert.NotNull(ignore);
        Assert.Equal(JsonIgnoreCondition.Never, ignore.Condition);
    }

    [Fact]
    public void Default_instance_has_null_value_and_serializes_explicit_null()
    {
        var identity = new VehicleServiceIdentity();

        Assert.Null(identity.VehicleIdService);
        Assert.Equal("{\"vehicleIdService\":null}", JsonSerializer.Serialize(identity));
    }

    [Fact]
    public void Explicit_null_serializes_as_named_null_property()
    {
        var identity = new VehicleServiceIdentity { VehicleIdService = null };

        Assert.Equal("{\"vehicleIdService\":null}", JsonSerializer.Serialize(identity));
    }

    [Fact]
    public void Missing_property_deserializes_to_null()
    {
        var identity = JsonSerializer.Deserialize<VehicleServiceIdentity>("{}");

        Assert.NotNull(identity);
        Assert.Null(identity.VehicleIdService);
    }

    [Fact]
    public void Explicit_null_property_deserializes_to_null()
    {
        var identity = JsonSerializer.Deserialize<VehicleServiceIdentity>("{\"vehicleIdService\":null}");

        Assert.NotNull(identity);
        Assert.Null(identity.VehicleIdService);
    }

    [Theory]
    [MemberData(nameof(StringValues))]
    public void String_input_deserializes_without_normalization_or_validation(string value)
    {
        string json = "{\"vehicleIdService\":" + JsonSerializer.Serialize(value) + "}";

        var identity = JsonSerializer.Deserialize<VehicleServiceIdentity>(json);

        Assert.NotNull(identity);
        Assert.Equal(value, identity.VehicleIdService);
    }

    [Theory]
    [MemberData(nameof(StringValues))]
    public void Non_null_strings_serialize_with_exact_name_and_roundtrip_unchanged(string value)
    {
        var identity = new VehicleServiceIdentity { VehicleIdService = value };

        string json = JsonSerializer.Serialize(identity);
        using var document = JsonDocument.Parse(json);
        var property = Assert.Single(document.RootElement.EnumerateObject());
        Assert.Equal("vehicleIdService", property.Name);
        Assert.False(document.RootElement.TryGetProperty("VehicleIdService", out _));
        Assert.Equal(JsonValueKind.String, property.Value.ValueKind);
        Assert.Equal(value, property.Value.GetString());

        var roundtripped = JsonSerializer.Deserialize<VehicleServiceIdentity>(json);
        Assert.NotNull(roundtripped);
        Assert.Equal(value, roundtripped.VehicleIdService);
    }
}
