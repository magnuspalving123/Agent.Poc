using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using DealerOrders.Api.Contracts;

namespace DealerOrders.Api.Tests.Unit;

public class VehicleServiceIdentityTests
{
    [Fact]
    public void Contract_is_public_sealed_record_with_nullable_init_property_and_json_declarations()
    {
        Type type = typeof(VehicleServiceIdentity);
        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.NotNull(type.GetMethod("<Clone>$"));

        PropertyInfo property = Assert.Single(type.GetProperties());
        Assert.Equal("VehicleIdService", property.Name);
        Assert.Equal(typeof(string), property.PropertyType);
        Assert.True(property.GetMethod!.IsPublic);
        Assert.True(property.SetMethod!.IsPublic);
        Assert.Contains(typeof(IsExternalInit), property.SetMethod.ReturnParameter.GetRequiredCustomModifiers());
        Assert.Equal(NullabilityState.Nullable, new NullabilityInfoContext().Create(property).ReadState);
        Assert.Equal("vehicleIdService", property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name);
        Assert.Equal(JsonIgnoreCondition.Never, property.GetCustomAttribute<JsonIgnoreAttribute>()?.Condition);

        var identity = new VehicleServiceIdentity();
        Assert.Equal(identity, identity with { });
    }

    [Fact]
    public void Default_instance_has_null_value_and_serializes_as_explicit_null()
    {
        var identity = new VehicleServiceIdentity();

        Assert.Null(identity.VehicleIdService);
        Assert.Equal("{\"vehicleIdService\":null}", JsonSerializer.Serialize(identity));
    }

    [Fact]
    public void Explicit_null_serializes_as_explicit_null()
    {
        var identity = new VehicleServiceIdentity { VehicleIdService = null };

        Assert.Equal("{\"vehicleIdService\":null}", JsonSerializer.Serialize(identity));
    }

    [Fact]
    public void Property_level_null_inclusion_overrides_global_null_omission()
    {
        var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        Assert.Equal("{\"vehicleIdService\":null}", JsonSerializer.Serialize(new VehicleServiceIdentity(), options));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"vehicleIdService\":null}")]
    public void Missing_or_explicit_null_deserializes_to_null(string json)
    {
        var identity = JsonSerializer.Deserialize<VehicleServiceIdentity>(json);

        Assert.NotNull(identity);
        Assert.Null(identity.VehicleIdService);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    [InlineData("mIxEdCaSe")]
    [InlineData("Ångström 車 🚗")]
    [InlineData("ordinary text, not a VIN!")]
    [InlineData("VW47586863")]
    [InlineData("  surrounding whitespace  ")]
    public void Strings_serialize_under_exact_name_and_roundtrip_unchanged(string value)
    {
        AssertPreserved(value);
    }

    [Fact]
    public void Long_string_serializes_and_roundtrips_without_truncation_or_rejection()
    {
        AssertPreserved(new string('x', 100_000));
    }

    private static void AssertPreserved(string value)
    {
        var identity = new VehicleServiceIdentity { VehicleIdService = value };

        string json = JsonSerializer.Serialize(identity);
        using JsonDocument document = JsonDocument.Parse(json);
        JsonProperty property = Assert.Single(document.RootElement.EnumerateObject());
        Assert.Equal("vehicleIdService", property.Name);
        Assert.Equal(JsonValueKind.String, property.Value.ValueKind);
        Assert.Equal(value, property.Value.GetString());

        var roundtripped = JsonSerializer.Deserialize<VehicleServiceIdentity>(json);
        Assert.NotNull(roundtripped);
        Assert.Equal(value, roundtripped.VehicleIdService);
    }
}
