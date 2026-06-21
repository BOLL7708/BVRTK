using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using BVRTK.Data.Request;
using BVRTK.Data.Request.Params;
using BVRTK.Data.Response;

namespace BVRTK.Components.Server;

public static class JsonHandler
{
    private static bool IsBatch(string jsonString)
    {
        return jsonString.Trim().StartsWith('[');
    }

    public static Tuple<List<JsonRpcResult>, bool> DecodeMessage(string jsonString)
    {
        var result = new List<JsonRpcResult>();
        var isBatch = IsBatch(jsonString);
        if (isBatch)
        {
            // Batch
            try
            {
                var list = JsonSerializer.Deserialize<List<JsonRpcRequest>>(jsonString, JsonRpcRequestSerializerContext.Default.ListJsonRpcRequest) ?? [];
                foreach (var item in list)
                {
                    result.Add(new JsonRpcResult(null, item));
                }
            }
            catch (Exception ex)
            {
                result.Add(new JsonRpcResult($"[{ex.Source}] {ex.Message}", null));
            }
        }
        else
        {
            // Single
            try
            {
                var instance = JsonSerializer.Deserialize<JsonRpcRequest>(jsonString, JsonRpcRequestSerializerContext.Default.JsonRpcRequest);
                if (instance != null)
                {
                    // Successfully parsed incoming message
                    result.Add(new JsonRpcResult(null, instance));
                }
                else
                {
                    // Not sure if this can even happen, if it fails it probably throws and is caught below.
                    result.Add(new JsonRpcResult("Unable to deserialize message.", null));
                }
            }
            catch (Exception ex)
            {
                result.Add(new JsonRpcResult($"[{ex.Source}] {ex.Message}", null));
            }
        }

        return new Tuple<List<JsonRpcResult>, bool>(result, isBatch);
    }

    public static string EncodeMessage(List<JsonRpcResponse> responseList, bool isBatch)
    {
        string message;
        try
        {
            message = isBatch 
                ? JsonSerializer.Serialize(responseList, JsonRpcResponseSerializerContext.Default.ListJsonRpcResponse) 
                : JsonSerializer.Serialize(responseList.First(), JsonRpcResponseSerializerContext.Default.JsonRpcResponse);
        }
        catch (Exception ex)
        {
            var errorMessage = ResponseUtils.GetErrorMessage(ResponseUtils.EJsonRpcErrorCode.FailedSerialization);
            message = $$"""{"jsonrpc":"2.0","error":{"code":100,"message":"{{errorMessage}} [{{ex.Source}}] ({{ex.Message}})"} }""";
        }

        return message;
    }

    public static JsonRpcResultParams<T> DecodeParams<T>(JsonElement? jsonElement, JsonTypeInfo<T> jsonTypeInfo) where T : class, new()
    {
        try
        {
            var decodedParams = jsonElement is JsonElement element
                ? element.Deserialize(jsonTypeInfo)
                : null;
            return new JsonRpcResultParams<T>(decodedParams, null);
        }
        catch (Exception ex)
        {
            return new JsonRpcResultParams<T>(null, ex);
        }
    }

    public static JsonRpcResultParams<T> DecodeParamsOrDefault<T>(JsonElement? jsonElement, JsonTypeInfo<T> jsonTypeInfo) where T : class, new()
    {
        try
        {
            var decodedParams = jsonElement is JsonElement element
                ? element.Deserialize(jsonTypeInfo)
                : new T();
            return new JsonRpcResultParams<T>(decodedParams, null);
        }
        catch (Exception ex)
        {
            return new JsonRpcResultParams<T>(new T(), ex);
        }
    }
}

public readonly struct JsonRpcResult(string? error, JsonRpcRequest? result)
{
    [MemberNotNullWhen(true, nameof(Result))]
    public bool IdExists => Result?.Id != null;
    [MemberNotNullWhen(true, nameof(Result))]
    public bool ParamsExists => Result?.Params != null;
    public string? Error { get; } = error;
    public JsonRpcRequest? Result { get; } = result;
}

public readonly struct JsonRpcResultParams<T>(T? result, Exception? exception) where T : class, new()
{
    [MemberNotNullWhen(true, nameof(Result))]
    [MemberNotNullWhen(false, nameof(Exception))]
    public bool Success => Result != null && Exception == null;
    public T? Result { get; }  = result;
    public Exception? Exception { get; } = exception;
}