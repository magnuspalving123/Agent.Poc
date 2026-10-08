using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using DealerOrders.Api.Contracts;

namespace DealerOrders.Api.Tests.Unit;

public class VehicleServiceIdentityTests
{
    [Fact]
    public void Contract_is_public_sealed_record_with_nullable_string_init_property_defaulting_to_null()
    {
        var type = typeof(VehicleServiceIdentity);
        var identity = new VehicleServiceIdentity();
        var property = type.GetProperty(nameof(VehicleServiceIdentity.VehicleIdService));

        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.NotNull(type.GetMethod("<Clone>$"));
        Assert.Equal(identity, identity with { });
        Assert.NotNull(property);
        Assert.Equal(typeof(string), property.PropertyType);
        Assert.Equal(NullabilityState.Nullable, new NullabilityInfoContext().Create(property).ReadState);
        Assert.NotNull(property.GetMethod);
        Assert.True(property.GetMethod.IsPublic);
        Assert.NotNull(property.SetMethod);
        Assert.True(property.SetMethod.IsPublic);
        Assert.Contains(typeof(IsExternalInit), property.SetMethod.ReturnParameter.GetRequiredCustomModifiers());
        Assert.Null(identity.VehicleIdService);
    }

    [Fact]
    public void Property_declares_exact_json_name_and_never_ignore_condition()
    {
        var property = typeof(VehicleServiceIdentity).GetProperty(nameof(VehicleServiceIdentity.VehicleIdService));
        Assert.NotNull(property);

        var name = property.GetCustomAttribute<JsonPropertyNameAttribute>();
        var ignore = property.GetCustomAttribute<JsonIgnoreAttribute>();

        Assert.NotNull(name);
        Assert.Equal("vehicleIdService", name.Name);
        Assert.NotNull(ignore);
        Assert.Equal(JsonIgnoreCondition.Never, ignore.Condition);
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

    [Fact]
    public void Default_serialization_includes_null_with_exact_property_name()
    {
        var identity = new VehicleServiceIdentity();

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(identity));

        var property = Assert.Single(json.RootElement.EnumerateObject());
        Assert.Equal("vehicleIdService", property.Name);
        Assert.Equal(JsonValueKind.Null, property.Value.ValueKind);
        Assert.False(json.RootElement.TryGetProperty("VehicleIdService", out _));
    }

    [Fact]
    public void Property_level_declarations_override_global_null_suppression_and_naming_policy()
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(new VehicleServiceIdentity(), options));

        var property = Assert.Single(json.RootElement.EnumerateObject());
        Assert.Equal("vehicleIdService", property.Name);
        Assert.Equal(JsonValueKind.Null, property.Value.ValueKind);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    [InlineData("mIxEdCaSe")]
    [InlineData("Æøå 日本語 🚗")]
    [InlineData("ordinary text, not a VIN!")]
    [InlineData("VW47586863")]
    [InlineData("  Vw47586863  ")]
    [InlineData("quotes \" and backslash \\ and newline\n")]
    public void Strings_roundtrip_exactly_without_validation_or_normalization(string value)
    {
        var identity = new VehicleServiceIdentity { VehicleIdService = value };

        string serialized = JsonSerializer.Serialize(identity);
        using var json = JsonDocument.Parse(serialized);
        var restored = JsonSerializer.Deserialize<VehicleServiceIdentity>(serialized);

        var property = Assert.Single(json.RootElement.EnumerateObject());
        Assert.Equal("vehicleIdService", property.Name);
        Assert.Equal(JsonValueKind.String, property.Value.ValueKind);
        Assert.Equal(value, property.Value.GetString());
        Assert.NotNull(restored);
        Assert.Equal(value, restored.VehicleIdService);
    }

    [Fact]
    public void Long_string_roundtrips_without_truncation_or_rejection()
    {
        string value = new('x', 100_000);
        var identity = new VehicleServiceIdentity { VehicleIdService = value };

        string serialized = JsonSerializer.Serialize(identity);
        using var json = JsonDocument.Parse(serialized);
        var restored = JsonSerializer.Deserialize<VehicleServiceIdentity>(serialized);

        Assert.Equal(value, json.RootElement.GetProperty("vehicleIdService").GetString());
        Assert.NotNull(restored);
        Assert.Equal(value, restored.VehicleIdService);
    }
}
