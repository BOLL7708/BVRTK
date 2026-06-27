using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using BVRTK.Components.Server.Request;
using BVRTK.Components.Server.Response;
using JsonRpcRequestSerializerContext = BVRTK.Components.Server.Request.JsonRpcRequestSerializerContext;
using JsonRpcResponseSerializerContext = BVRTK.Components.Server.Response.JsonRpcResponseSerializerContext;

namespace BVRTK.Components.Server;

public static class JsonHandler
{
    private static bool IsBatch(string jsonString)
    {
        return jsonString.Trim().StartsWith('[');
    }

    /// <summary>
    /// Will decode an incoming JSON string into JSON RPC 2.0 message objects.
    /// Supports both single messages and batches, that is lists of messages.
    /// </summary>
    /// <param name="jsonString"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Will take a list of response instances and encode them into a JSON string.
    /// This supports single messages and a batch of messages, handles errors too.
    /// </summary>
    /// <param name="responseList"></param>
    /// <param name="isBatch"></param>
    /// <returns></returns>
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
            const EJsonRpcErrorCode errorCode = EJsonRpcErrorCode.FailedSerialization;
            var errorMessage = ErrorBuilder.GetErrorMessage(errorCode);
            
            // The below matches JsonRpcResponse with JsonRpcError, what ErrorBuilder.BuildWithException() would output.
            message = $$"""
                        {
                            "jsonrpc":"2.0",
                            "error":{
                                "code":{{errorCode}},
                                "message":"{{errorMessage}}"
                                "data":{
                                    "requestMethod":"{{Enum.GetName(EJsonRpcMethod.Unknown)}}",
                                    "exceptionMessage":"{{ex.Message}}",
                                    "exceptionType":"{{ex.GetType().Name}}",
                                    "exceptionSource":"{{ex.Source}}",
                                    "exceptionStackTrace":"{{ErrorBuilder.GetLocalStackTrace(ex.StackTrace)}}"
                                }
                            } 
                        }
                        """;
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