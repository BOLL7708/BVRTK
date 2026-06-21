using System.Text.Json.Serialization;

namespace BVRTK.Data.Request.Params;

// Append this list with all classes that should be possible to serialize/deserialize.
[JsonSerializable(
    typeof(ShowBindingEditor)
)]
[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    IncludeFields = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    UseStringEnumConverter = true
)]
public partial class ParamsJsonSerializerContext : JsonSerializerContext
{
}