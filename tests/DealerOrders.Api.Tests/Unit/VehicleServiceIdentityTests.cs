using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using DealerOrders.Api.Contracts;

namespace DealerOrders.Api.Tests.Unit;

public class VehicleServiceIdentityTests
{
    [Fact]
    public void Contract_is_public_sealed_record_with_nullable_init_only_string_property()
    {
        Type type = typeof(VehicleServiceIdentity);
        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.Equal("VehicleServiceIdentity", type.Name);

        PropertyInfo property = Assert.Single(type.GetProperties());
        Assert.Equal("VehicleIdService", property.Name);
        Assert.Equal(typeof(string), property.PropertyType);
        Assert.True(property.GetMethod!.IsPublic);
        Assert.True(property.SetMethod!.IsPublic);
        Assert.Contains(typeof(IsExternalInit), property.SetMethod.ReturnParameter.GetRequiredCustomModifiers());
        Assert.Equal(NullabilityState.Nullable, new NullabilityInfoContext().Create(property).ReadState);

        var identity = new VehicleServiceIdentity { VehicleIdService = "VW47586863" };
        Assert.Equal(identity, identity with { });
    }

    [Fact]
    public void Default_construction_leaves_vehicle_id_service_null()
    {
        var identity = new VehicleServiceIdentity();

        Assert.Null(identity.VehicleIdService);
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
    public void Property_declares_exact_json_name_and_never_ignore_condition()
    {
        PropertyInfo property = typeof(VehicleServiceIdentity).GetProperty(nameof(VehicleServiceIdentity.VehicleIdService))!;

        Assert.Equal("vehicleIdService", property.GetCustomAttribute<JsonPropertyNameAttribute>()!.Name);
        Assert.Equal(JsonIgnoreCondition.Never, property.GetCustomAttribute<JsonIgnoreAttribute>()!.Condition);
    }

    [Fact]
    public void Default_serialization_emits_only_exact_contract_member_with_null()
    {
        string json = JsonSerializer.Serialize(new VehicleServiceIdentity());
        using JsonDocument document = JsonDocument.Parse(json);

        JsonProperty member = Assert.Single(document.RootElement.EnumerateObject());
        Assert.Equal("vehicleIdService", member.Name);
        Assert.Equal(JsonValueKind.Null, member.Value.ValueKind);
    }

    [Fact]
    public void Null_inclusion_and_exact_name_override_global_ignore_and_naming_options()
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        string json = JsonSerializer.Serialize(new VehicleServiceIdentity(), options);
        using JsonDocument document = JsonDocument.Parse(json);

        JsonProperty member = Assert.Single(document.RootElement.EnumerateObject());
        Assert.Equal("vehicleIdService", member.Name);
        Assert.Equal(JsonValueKind.Null, member.Value.ValueKind);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    [InlineData("mIxEdCaSe")]
    [InlineData("Æøå 日本語 🚗")]
    [InlineData("ordinary text")]
    [InlineData("VW47586863")]
    [InlineData("  not-a-VIN!?  ")]
    public void Deserialization_and_roundtrip_preserve_strings_exactly(string value)
    {
        AssertStringPreserved(value);
    }

    [Fact]
    public void Deserialization_and_roundtrip_preserve_long_string_without_truncation()
    {
        AssertStringPreserved(new string('x', 100_000));
    }

    private static void AssertStringPreserved(string value)
    {
        string inputJson = "{\"vehicleIdService\":" + JsonSerializer.Serialize(value) + "}";
        var deserialized = JsonSerializer.Deserialize<VehicleServiceIdentity>(inputJson);
        Assert.NotNull(deserialized);
        Assert.Equal(value, deserialized.VehicleIdService);

        var identity = new VehicleServiceIdentity { VehicleIdService = value };
        string json = JsonSerializer.Serialize(identity);
        using JsonDocument document = JsonDocument.Parse(json);
        JsonProperty member = Assert.Single(document.RootElement.EnumerateObject());
        Assert.Equal("vehicleIdService", member.Name);
        Assert.Equal(JsonValueKind.String, member.Value.ValueKind);
        Assert.Equal(value, member.Value.GetString());

        var roundtripped = JsonSerializer.Deserialize<VehicleServiceIdentity>(json);
        Assert.NotNull(roundtripped);
        Assert.Equal(value, roundtripped.VehicleIdService);
    }
}
