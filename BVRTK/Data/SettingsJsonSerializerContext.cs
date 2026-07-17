using System.Text.Json.Serialization;
using BVRTK.Data.Setting;

namespace BVRTK.Data;

// Append this list with all root classes that should be possible to serialize/deserialize.
[JsonSerializable(typeof(Settings))]
[JsonSerializable(typeof(Server))]
[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    IncludeFields = false,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    WriteIndented = true
)]
public partial class SettingsJsonSerializerContext : JsonSerializerContext
{
}