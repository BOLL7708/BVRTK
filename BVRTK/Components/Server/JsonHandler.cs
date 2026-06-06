using System.Text.Json;
using BVRTK.Data.Request;
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
            if (isBatch)
            {
                message = JsonSerializer.Serialize(responseList, JsonRpcResponseSerializerContext.Default.ListJsonRpcResponse);
            }
            else
            {
                message = JsonSerializer.Serialize(responseList.First(), JsonRpcResponseSerializerContext.Default.JsonRpcResponse);
            }
        }
        catch (Exception ex)
        {
            message = $$"""{"jsonrpc":"2.0","error":{"code":100,"message":"[{{ex.Source}}] {{ex.Message}}"} }""";
        }

        return message;
    }
}

public readonly struct JsonRpcResult(string? error, JsonRpcRequest? result)
{
    public string? Error { get; } = error;
    public JsonRpcRequest? Result { get; } = result;
}