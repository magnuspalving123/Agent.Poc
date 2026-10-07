using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using DealerOrders.Api.Contracts;

namespace DealerOrders.Api.Tests.Unit;

public class VehicleServiceIdentityTests
{
    [Fact]
    public void Contract_is_public_sealed_record_with_nullable_string_init_property_and_null_default()
    {
        var type = typeof(VehicleServiceIdentity);
        var property = type.GetProperty(nameof(VehicleServiceIdentity.VehicleIdService));

        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.NotNull(property);
        Assert.Equal(typeof(string), property.PropertyType);
        Assert.True(property.GetMethod!.IsPublic);
        Assert.True(property.SetMethod!.IsPublic);
        Assert.Contains(typeof(IsExternalInit), property.SetMethod.ReturnParameter.GetRequiredCustomModifiers());
        Assert.Equal(NullabilityState.Nullable, new NullabilityInfoContext().Create(property).ReadState);

        var identity = new VehicleServiceIdentity();
        Assert.Null(identity.VehicleIdService);
        Assert.Equal(identity, identity with { });
        Assert.NotSame(identity, identity with { });
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
    public void Default_serialization_includes_exact_property_name_with_null_value()
    {
        var json = JsonSerializer.Serialize(new VehicleServiceIdentity());
        using var document = JsonDocument.Parse(json);

        var property = Assert.Single(document.RootElement.EnumerateObject());
        Assert.Equal("vehicleIdService", property.Name);
        Assert.Equal(JsonValueKind.Null, property.Value.ValueKind);
        Assert.False(document.RootElement.TryGetProperty("VehicleIdService", out _));
    }

    [Fact]
    public void Property_declarations_enforce_exact_name_and_null_inclusion_despite_global_options()
    {
        var property = typeof(VehicleServiceIdentity).GetProperty(nameof(VehicleServiceIdentity.VehicleIdService))!;
        Assert.Equal("vehicleIdService", property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name);
        Assert.Equal(JsonIgnoreCondition.Never, property.GetCustomAttribute<JsonIgnoreAttribute>()?.Condition);

        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        var json = JsonSerializer.Serialize(new VehicleServiceIdentity(), options);
        using var document = JsonDocument.Parse(json);

        var serializedProperty = Assert.Single(document.RootElement.EnumerateObject());
        Assert.Equal("vehicleIdService", serializedProperty.Name);
        Assert.Equal(JsonValueKind.Null, serializedProperty.Value.ValueKind);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    [InlineData("mIxEdCaSe")]
    [InlineData("Æøå 日本語 🚗")]
    [InlineData("ordinary text, not a VIN!")]
    [InlineData("VW47586863")]
    [InlineData("  padded text  ")]
    [InlineData("quotes: \" and slash: \\ and newline: \n")]
    public void String_values_are_preserved_in_serialized_json_and_default_roundtrip(string value)
    {
        AssertDefaultRoundtrip(value);
    }

    [Fact]
    public void Long_string_is_preserved_in_serialized_json_and_default_roundtrip()
    {
        AssertDefaultRoundtrip(new string('x', 100_000));
    }

    private static void AssertDefaultRoundtrip(string value)
    {
        var identity = new VehicleServiceIdentity { VehicleIdService = value };
        var json = JsonSerializer.Serialize(identity);
        using var document = JsonDocument.Parse(json);

        var property = Assert.Single(document.RootElement.EnumerateObject());
        Assert.Equal("vehicleIdService", property.Name);
        Assert.Equal(JsonValueKind.String, property.Value.ValueKind);
        Assert.Equal(value, property.Value.GetString());

        var roundtripped = JsonSerializer.Deserialize<VehicleServiceIdentity>(json);
        Assert.NotNull(roundtripped);
        Assert.Equal(value, roundtripped.VehicleIdService);
    }
}
