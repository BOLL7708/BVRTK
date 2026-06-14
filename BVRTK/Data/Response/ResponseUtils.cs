using System.Text.Json;
using BVRTK.Components.Server;
using BVRTK.Resources;

namespace BVRTK.Data.Response;

public static class ResponseUtils
{
    public enum EJsonRpcErrorCode
    {
        InvalidRequestBody = 1,
        InvalidJsonRpcVersion = 2,
        UnhandledMethod = 3
    };

    private static JsonRpcResponse BuildGeneric(
        string? id = null,
        bool? done = null,
        string? message = null,
        List<string>? list = null,
        Dictionary<string, string>? dictionary = null
    )
    {
        return new JsonRpcResponse
        {
            Result = JsonSerializer.SerializeToNode(new GenericResponse
            {
                Done = done,
                Message = message,
                List = list,
                Dictionary = dictionary
            }, JsonRpcResponseSerializerContext.Default.GenericResponse),
            Id = id
        };
    }

    public static JsonRpcResponse BuildStatus(string? id, bool? done, string message)
    {
        return BuildGeneric(id, done, message);
    }

    public static JsonRpcResponse BuildMessage(string? id, string message)
    {
        return BuildGeneric(id, message: message);
    }

    public static JsonRpcResponse BuildList(string? id, List<string> list)
    {
        return BuildGeneric(id, list: list);
    }

    public static JsonRpcResponse BuildDictionary(string? id, Dictionary<string, string> dictionary)
    {
        return BuildGeneric(id, dictionary: dictionary);
    }
    
    private static string GetErrorMessage(EJsonRpcErrorCode code)
    {
        return code switch
        {
            EJsonRpcErrorCode.InvalidRequestBody => JsonRpcErrors.InvalidRequestBody,
            EJsonRpcErrorCode.InvalidJsonRpcVersion => JsonRpcErrors.InvalidJsonRpcVersion,
            EJsonRpcErrorCode.UnhandledMethod => JsonRpcErrors.UnhandledMethod,
            _ => JsonRpcErrors.Unknown
        };
    }
    
    public static JsonRpcResponse BuildError(JsonRpcResult result, EJsonRpcErrorCode code, string? auxMessage = null)
    {
        var prefix = result.Result?.Method != null ? $"[{result.Result.Method}] " : ""; 
        var message = GetErrorMessage(code);
        var suffix = auxMessage == null ? "" : $" ({auxMessage})";
        return new JsonRpcResponse
        {
            Error = new JsonRpcError
            {
                Code = (int) code,
                Message = $"{prefix}{message}{suffix}"
            },
            Id = result.Result?.Id
        };
    }
}