using System.Text.Json;
using System.Text.Json.Serialization;

namespace BVRTK.Components.Server;

public static class JsonHandler
{
    private static bool IsBatch(string jsonString)
    {
        return jsonString.Trim().StartsWith('[');
    }
    
    public static Tuple<List<JsonRpcResult>, bool> ParseMessage(string jsonString)
    {
        var result = new List<JsonRpcResult>();
        var isBatch = IsBatch(jsonString);
        if (isBatch)
        {
            // Batch
            try
            {
                var list = JsonSerializer.Deserialize<List<JsonRpcMessage>>(jsonString, JsonRpcSourceGenerationContext.Default.ListJsonRpcMessage) ?? [];
                foreach (var item in list)
                {
                    result.Add(new JsonRpcResult(true, null, item));
                }
            }
            catch (Exception ex)
            {
                result.Add(new JsonRpcResult(false, $"[{ex.Source}] {ex.Message}", null));
            }
        }
        else
        {
            // Single
            try
            {
                var instance = JsonSerializer.Deserialize<JsonRpcMessage>(jsonString, JsonRpcSourceGenerationContext.Default.JsonRpcMessage);
                if (instance != null)
                {
                    // Successfully parsed incoming message
                    result.Add(new JsonRpcResult(true, null, instance));
                }
                else
                {
                    // Not sure if this can even happen, if it fails it probably throws and is caught below.
                    result.Add(new JsonRpcResult(false, "Unable to deserialize message.", null));
                }
            }
            catch (Exception ex)
            {
                result.Add(new JsonRpcResult(false, $"[{ex.Source}] {ex.Message}", null));
            }
        }

        return new Tuple<List<JsonRpcResult>, bool>(result, isBatch);
    }
}

public class JsonRpcMessage
{
    [JsonPropertyName("jsonrpc")]
    public required string JsonRpc { get; init; }

    [JsonPropertyName("method")]
    public required EMethod Method { get; init; }

    [JsonPropertyName("id")] public string? Id { get; init; }

    [JsonPropertyName("params")] public JsonElement? Params { get; init; }
}

public readonly struct JsonRpcResult(bool success, string? error, JsonRpcMessage? result)
{
    public bool Success { get;} = success;
    public string? Error { get; } = error;
    public JsonRpcMessage? Result { get; } = result;
}

public enum EMethod
{
    ShowBindingsEditor
}

[JsonSourceGenerationOptions(
    // IncludeFields = true,
    // NumberHandling = JsonNumberHandling.AllowReadingFromString,
    // GenerationMode = JsonSourceGenerationMode.Metadata | JsonSourceGenerationMode.Serialization,
    UseStringEnumConverter = true
)]
[JsonSerializable(typeof(JsonRpcMessage))]
[JsonSerializable(typeof(List<JsonRpcMessage>))]
public partial class JsonRpcSourceGenerationContext : JsonSerializerContext
{
}