using System.Text.Json;
using System.Text.RegularExpressions;
using BVRTK.Components.Server;
using BVRTK.Resources;

namespace BVRTK.Data.Response;

public static class ResponseUtils
{
    public enum EJsonRpcErrorCode
    {
        InvalidRequestBody = 1,
        InvalidJsonRpcVersion = 2,
        UnhandledMethod = 3,
        FailedSerialization = 4,
        SteamVrError = 5,
        InvalidBodyParams = 6,
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

    public static string GetErrorMessage(EJsonRpcErrorCode code)
    {
        return code switch
        {
            EJsonRpcErrorCode.InvalidRequestBody => JsonRpcErrors.InvalidRequestBody,
            EJsonRpcErrorCode.InvalidJsonRpcVersion => JsonRpcErrors.InvalidJsonRpcVersion,
            EJsonRpcErrorCode.UnhandledMethod => JsonRpcErrors.UnhandledMethod,
            EJsonRpcErrorCode.FailedSerialization => JsonRpcErrors.FailedSerialization,
            EJsonRpcErrorCode.SteamVrError => JsonRpcErrors.SteamVrError,
            EJsonRpcErrorCode.InvalidBodyParams => JsonRpcErrors.InvalidBodyParams,
            _ => JsonRpcErrors.Unknown
        };
    }

    public static JsonRpcResponse BuildError(JsonRpcResult result, EJsonRpcErrorCode code, string errorMessage)
    {
        var message = GetErrorMessage(code);
        return new JsonRpcResponse
        {
            Error = new JsonRpcError
            {
                Code = (int)code,
                Message = message,
                Data = new JsonRpcErrorData
                {
                    RequestMethod = result.Result?.Method != null ? Enum.GetName(result.Result.Method) : null,
                    ExceptionMessage = errorMessage
                }
            },
            Id = result.Result?.Id
        };
    }

    public static JsonRpcResponse BuildError(JsonRpcResult result, EJsonRpcErrorCode code, Exception? exception = null)
    {
        var message = GetErrorMessage(code);
        return new JsonRpcResponse
        {
            Error = new JsonRpcError
            {
                Code = (int)code,
                Message = message,
                Data = new JsonRpcErrorData
                {
                    RequestMethod = result.Result?.Method != null ? Enum.GetName(result.Result.Method) : null,
                    ExceptionMessage = exception?.Message,
                    ExceptionType = exception?.GetType().Name,
                    ExceptionSource = exception?.Source,
                    ExceptionStackTrace = GetLocalStackTrace(exception?.StackTrace)
                }
            },
            Id = result.Result?.Id
        };
    }

    private static string[] GetLocalStackTrace(string? stackTrace)
    {
        var regex = new Regex(@"\s*[\r\n|\r|\n]\s*");
        var rows = regex.Split(stackTrace?.Trim() ?? "");
        // var localStack = rows.ToList()
        //     .FindAll(it => it.StartsWith("at BVRTK") || it.StartsWith("at EasyOpenVR"));
        // return localStack.ToArray();
        return rows;
    }
}