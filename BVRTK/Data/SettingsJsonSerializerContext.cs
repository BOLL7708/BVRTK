using System.Text.Json.Serialization;

namespace BVRTK.Data;

// Append this list with all root classes that should be possible to serialize/deserialize.
[JsonSerializable(typeof(Settings))]
[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    IncludeFields = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    WriteIndented = true
)]
public partial class SettingsJsonSerializerContext : JsonSerializerContext
{
}