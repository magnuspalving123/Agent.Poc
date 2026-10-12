using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using DealerOrders.Api.Contracts;

namespace DealerOrders.Api.Tests.Unit;

public class VehicleServiceIdentityTests
{
    [Fact]
    public void Contract_is_public_sealed_record_with_nullable_init_property_and_null_default()
    {
        var type = typeof(VehicleServiceIdentity);
        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        var property = Assert.Single(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Equal("VehicleIdService", property.Name);
        Assert.Equal(typeof(string), property.PropertyType);
        Assert.Equal(NullabilityState.Nullable, new NullabilityInfoContext().Create(property).ReadState);
        Assert.True(property.GetMethod!.IsPublic);
        Assert.True(property.SetMethod!.IsPublic);
        Assert.Contains(typeof(IsExternalInit), property.SetMethod.ReturnParameter.GetRequiredCustomModifiers());

        var identity = new VehicleServiceIdentity();
        Assert.Null(identity.VehicleIdService);
        var copy = identity with { };
        Assert.NotSame(identity, copy);
        Assert.Equal(identity, copy);
    }

    [Fact]
    public void Property_declares_exact_json_name_and_explicit_null_inclusion()
    {
        var property = typeof(VehicleServiceIdentity).GetProperty(nameof(VehicleServiceIdentity.VehicleIdService))!;

        Assert.Equal("vehicleIdService", property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name);
        var ignore = property.GetCustomAttribute<JsonIgnoreAttribute>();
        Assert.NotNull(ignore);
        Assert.Equal(JsonIgnoreCondition.Never, ignore.Condition);
    }

    [Fact]
    public void Default_serialization_includes_null_under_exact_json_name()
    {
        string json = JsonSerializer.Serialize(new VehicleServiceIdentity());

        Assert.Equal("{\"vehicleIdService\":null}", json);
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
    [InlineData("vWAbC123xYz")]
    [InlineData("Æøå 車 🚗")]
    [InlineData("ordinary text, not a VIN")]
    [InlineData("VW47586863")]
    [InlineData("  surrounding whitespace  ")]
    public void Strings_serialize_with_exact_json_name_and_roundtrip_unchanged(string value)
    {
        AssertRoundtrip(value);
    }

    [Fact]
    public void Long_string_serializes_and_roundtrips_without_truncation()
    {
        AssertRoundtrip(new string('x', 100_000));
    }

    private static void AssertRoundtrip(string value)
    {
        var identity = new VehicleServiceIdentity { VehicleIdService = value };
        string json = JsonSerializer.Serialize(identity);
        using var document = JsonDocument.Parse(json);
        var property = Assert.Single(document.RootElement.EnumerateObject());
        Assert.Equal("vehicleIdService", property.Name);
        Assert.False(document.RootElement.TryGetProperty("VehicleIdService", out _));
        Assert.Equal(JsonValueKind.String, property.Value.ValueKind);
        Assert.Equal(value, property.Value.GetString());

        var restored = JsonSerializer.Deserialize<VehicleServiceIdentity>(json);
        Assert.NotNull(restored);
        Assert.Equal(value, restored.VehicleIdService);
    }
}
