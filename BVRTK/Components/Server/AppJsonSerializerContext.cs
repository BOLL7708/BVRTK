using System.Text.Json.Serialization;
using BVRTK.Data;

namespace BVRTK.Components.Server;

[JsonSerializable(typeof(Settings))]
[JsonSourceGenerationOptions(
    IncludeFields = true,
    GenerationMode = JsonSourceGenerationMode.Default,
    WriteIndented = false,
    PropertyNameCaseInsensitive = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters =
    [
        typeof(JsonStringEnumConverter<SuperServer.ServerStatus>) // TODO: Only as reference
    ]
)]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}