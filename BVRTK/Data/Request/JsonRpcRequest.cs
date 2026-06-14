using System.Text.Json;
using System.Text.Json.Serialization;

namespace BVRTK.Data.Request;

public enum EJsonRpcMethod
{
    Help,
    ListMethods,
    ShowBindingsEditor
}

/// <summary>
/// <a href="https://www.jsonrpc.org/specification#request_object">Reference</a> 
/// </summary>
/// <remarks>
/// A RPC is represented by sending a Request object to a Server.
/// </remarks>
public class JsonRpcRequest
{
    /// A String specifying the version of the JSON-RPC protocol.
    /// MUST be exactly "2.0".
    [JsonPropertyName("jsonrpc")]
    public required string JsonRpc { get; init; }

    /// A String containing the name of the method to be invoked.
    /// Method names that begin with the word rpc followed by a period character (U+002E or ASCII 46) are reserved for rpc-internal methods and extensions and MUST NOT be used for anything else.
    public required EJsonRpcMethod Method { get; init; }

    /// A Structured value that holds the parameter values to be used during the invocation of the method.
    /// This member MAY be omitted.
    public JsonElement? Params { get; init; }

    /// <summary>
    /// An identifier established by the Client that MUST contain a String, Number, or NULL value if included.
    /// If it is not included it is assumed to be a notification.
    /// The value SHOULD normally not be Null and Numbers SHOULD NOT contain fractional parts.
    /// </summary>
    /// <remarks>
    /// The Server MUST reply with the same value in the Response object if included. This member is used to correlate the context between the two objects.
    /// </remarks>
    public string? Id { get; init; }
}

[JsonSourceGenerationOptions(
    UseStringEnumConverter = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(JsonRpcRequest))]
[JsonSerializable(typeof(List<JsonRpcRequest>))]
public partial class JsonRpcRequestSerializerContext : JsonSerializerContext;