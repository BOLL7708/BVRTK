using System.Text.Json.Serialization;
using BVRTK.Components.Server.Request.Params;

namespace BVRTK.Components.Server.Request;

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