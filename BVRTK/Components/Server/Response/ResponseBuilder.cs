using System.Text.Json;
using System.Text.RegularExpressions;
using BVRTK.Resources;

namespace BVRTK.Components.Server.Response;

public class ResponseBuilder(string? id = null)
{
    private JsonRpcResponse BuildGeneric(
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

    public JsonRpcResponse BuildStatus(bool? done, string message)
    {
        return BuildGeneric(done, message);
    }

    public JsonRpcResponse BuildMessage(string message)
    {
        return BuildGeneric(message: message);
    }

    public JsonRpcResponse BuildList(List<string> list)
    {
        return BuildGeneric(list: list);
    }

    public JsonRpcResponse BuildDictionary(Dictionary<string, string> dictionary)
    {
        return BuildGeneric(dictionary: dictionary);
    }

    public static JsonRpcResponse BuildError(JsonRpcResult result, EJsonRpcErrorCode code)
    {
        return new JsonRpcResponse
        {
            Error = new ErrorBuilder(result, code).Build(),
            Id = result.Result?.Id
        };
    }
    
    public static JsonRpcResponse BuildErrorWithMessage(JsonRpcResult result, EJsonRpcErrorCode code, string? errorMessage)
    {
        return new JsonRpcResponse
        {
            Error = new ErrorBuilder(result, code).BuildWithMessage(errorMessage),
            Id = result.Result?.Id
        };
    }

    public static JsonRpcResponse BuildErrorWithException(JsonRpcResult result, EJsonRpcErrorCode code, Exception? exception)
    {
        return new JsonRpcResponse
        {
            Error = new ErrorBuilder(result, code).BuildWithException(exception),
            Id = result.Result?.Id
        };
    }
}