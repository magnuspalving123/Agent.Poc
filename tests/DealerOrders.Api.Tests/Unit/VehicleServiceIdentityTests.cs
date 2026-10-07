using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using DealerOrders.Api.Contracts;

namespace DealerOrders.Api.Tests.Unit;

public class VehicleServiceIdentityTests
{
    [Fact]
    public void Contract_is_public_sealed_record_with_nullable_init_only_string_defaulting_to_null()
    {
        Type contractType = typeof(VehicleServiceIdentity);
        Assert.True(contractType.IsPublic);
        Assert.True(contractType.IsSealed);

        PropertyInfo property = Assert.IsType<PropertyInfo>(
            contractType.GetProperty(nameof(VehicleServiceIdentity.VehicleIdService)), exactMatch: false);
        Assert.Equal(typeof(string), property.PropertyType);
        Assert.Equal(NullabilityState.Nullable, new NullabilityInfoContext().Create(property).ReadState);
        Assert.True(property.GetMethod!.IsPublic);
        Assert.True(property.SetMethod!.IsPublic);
        Assert.Contains(typeof(IsExternalInit), property.SetMethod.ReturnParameter.GetRequiredCustomModifiers());

        var identity = new VehicleServiceIdentity();
        Assert.Null(identity.VehicleIdService);
        Assert.Equal(identity, new VehicleServiceIdentity());
        Assert.Equal("mock", (identity with { VehicleIdService = "mock" }).VehicleIdService);
        Assert.NotNull(contractType.GetMethod("<Clone>$"));
    }

    [Fact]
    public void Property_declarations_specify_exact_json_name_and_never_ignore()
    {
        PropertyInfo property = typeof(VehicleServiceIdentity).GetProperty(nameof(VehicleServiceIdentity.VehicleIdService))!;

        Assert.Equal("vehicleIdService", property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name);
        Assert.Equal(JsonIgnoreCondition.Never, property.GetCustomAttribute<JsonIgnoreAttribute>()?.Condition);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"vehicleIdService\":null}")]
    public void Missing_or_explicit_null_json_deserializes_to_null(string json)
    {
        VehicleServiceIdentity identity = Assert.IsType<VehicleServiceIdentity>(
            JsonSerializer.Deserialize<VehicleServiceIdentity>(json));

        Assert.Null(identity.VehicleIdService);
    }

    [Fact]
    public void Default_serialization_includes_exact_property_name_with_null_value()
    {
        string json = JsonSerializer.Serialize(new VehicleServiceIdentity());
        using JsonDocument document = JsonDocument.Parse(json);

        Assert.Single(document.RootElement.EnumerateObject());
        Assert.True(document.RootElement.TryGetProperty("vehicleIdService", out JsonElement value));
        Assert.Equal(JsonValueKind.Null, value.ValueKind);
        Assert.False(document.RootElement.TryGetProperty("VehicleIdService", out _));
    }

    [Fact]
    public void Property_null_inclusion_overrides_global_null_ignore_and_naming_policy()
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        string json = JsonSerializer.Serialize(new VehicleServiceIdentity(), options);
        using JsonDocument document = JsonDocument.Parse(json);

        Assert.Single(document.RootElement.EnumerateObject());
        Assert.True(document.RootElement.TryGetProperty("vehicleIdService", out JsonElement value));
        Assert.Equal(JsonValueKind.Null, value.ValueKind);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    [InlineData("mIxEd CaSe")]
    [InlineData("Blåbær 日本語 🚗")]
    [InlineData("ordinary mock text !@# / not a VIN")]
    [InlineData("VW47586863")]
    [InlineData("  padded text  ")]
    public void String_values_are_preserved_in_json_and_default_roundtrip(string original)
    {
        var identity = new VehicleServiceIdentity { VehicleIdService = original };

        string json = JsonSerializer.Serialize(identity);
        using JsonDocument document = JsonDocument.Parse(json);
        Assert.Equal(original, document.RootElement.GetProperty("vehicleIdService").GetString());

        VehicleServiceIdentity restored = Assert.IsType<VehicleServiceIdentity>(
            JsonSerializer.Deserialize<VehicleServiceIdentity>(json));
        Assert.Equal(original, restored.VehicleIdService);
    }

    [Fact]
    public void Long_non_vin_string_is_preserved_in_json_and_default_roundtrip()
    {
        string original = new('x', 65536);
        var identity = new VehicleServiceIdentity { VehicleIdService = original };

        string json = JsonSerializer.Serialize(identity);
        using JsonDocument document = JsonDocument.Parse(json);
        Assert.Equal(original, document.RootElement.GetProperty("vehicleIdService").GetString());

        VehicleServiceIdentity restored = Assert.IsType<VehicleServiceIdentity>(
            JsonSerializer.Deserialize<VehicleServiceIdentity>(json));
        Assert.Equal(original, restored.VehicleIdService);
    }
}
