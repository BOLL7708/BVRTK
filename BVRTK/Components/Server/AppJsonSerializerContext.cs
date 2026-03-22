using System.Text.Json.Serialization;
using BVRTK.Data;

namespace BVRTK.Components.Server;

// Append this list with all classes that should be possible to serialize/deserialize.
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
        // Append this list with all enums that should be possible to serialize/deserialize
        typeof(JsonStringEnumConverter<SuperServer.ServerStatus>)
    ]
)]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}