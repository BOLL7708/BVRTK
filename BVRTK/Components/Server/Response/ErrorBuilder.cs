using System.Text.RegularExpressions;
using BVRTK.Resources;

namespace BVRTK.Components.Server.Response;

public class ErrorBuilder(JsonRpcResult result, EJsonRpcErrorCode code)
{
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
    
    public static string[] GetLocalStackTrace(string? stackTrace)
    {
        var regex = new Regex(@"\s*[\r\n|\r|\n]\s*");
        var rows = regex.Split(stackTrace?.Trim() ?? "");
        // var localStack = rows.ToList()
        //     .FindAll(it => it.StartsWith("at BVRTK") || it.StartsWith("at EasyOpenVR"));
        // return localStack.ToArray();
        return rows;
    }

    public JsonRpcError Build()
    {
        var message = GetErrorMessage(code);
        return new JsonRpcError
        {
            Code = (int)code,
            Message = message,
            Data = new JsonRpcErrorData
            {
                RequestMethod = result.Result?.Method != null ? Enum.GetName(result.Result.Method) : null
            }
        };
    }
    
    public JsonRpcError BuildWithMessage(string? errorMessage)
    {
        var message = GetErrorMessage(code);
        return new JsonRpcError
        {
            Code = (int)code,
            Message = message,
            Data = new JsonRpcErrorData
            {
                RequestMethod = result.Result?.Method != null ? Enum.GetName(result.Result.Method) : null,
                ExceptionMessage = errorMessage
            }
        };
    }
    
    public JsonRpcError BuildWithException(Exception? exception)
    {
        var message = GetErrorMessage(code);
        return new JsonRpcError
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
        };
    }
}

public enum EJsonRpcErrorCode
{
    InvalidRequestBody = 1,
    InvalidJsonRpcVersion = 2,
    UnhandledMethod = 3,
    FailedSerialization = 4,
    SteamVrError = 5,
    InvalidBodyParams = 6,
};