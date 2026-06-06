using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BVRTK.Data.Response;

/// <summary>
/// <a href="https://www.jsonrpc.org/specification#response_object">Reference</a> 
/// </summary>
/// <remarks>
/// When a rpc call is made, the Server MUST reply with a Response, except for in the case of Notifications.
/// The Response is expressed as a single JSON Object.
/// </remarks>
public class JsonRpcResponse
{
    /// A String specifying the version of the JSON-RPC protocol. MUST be exactly "2.0".
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; } = "2.0";

    /// <para>This member is REQUIRED on success.</para>
    /// <para>This member MUST NOT exist if there was an error invoking the method.</para>
    /// <para>The value of this member is determined by the method invoked on the Server.</para>
    public JsonNode? Result { get; init; }

    /// <para>This member is REQUIRED on error.</para>
    /// <para>This member MUST NOT exist if there was no error triggered during invocation.</para>
    /// <para>The value for this member MUST be an Object as defined in section 5.1.</para>
    public JsonRpcError? Error { get; init; }

    /// <para>This member is REQUIRED.</para>
    /// <para>It MUST be the same as the value of the id member in the Request Object.</para>
    /// <para>If there was an error in detecting the id in the Request object (e.g. Parse error/Invalid Request), it MUST be Null.</para>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Id { get; init; }
}

/// <summary>
/// <a href="https://www.jsonrpc.org/specification#error_object">Reference</a>
/// </summary>
/// <remarks>
/// When a rpc call encounters an error, the Response Object MUST contain the error member with a value that is an Object.
/// </remarks>
public class JsonRpcError
{
    /// <para>A Number that indicates the error type that occurred.</para>
    /// <para>This MUST be an integer.</para>
    public required int Code { get; init; }

    /// <para>A String providing a short description of the error.</para>
    /// <para>The message SHOULD be limited to a concise single sentence.</para>
    public required string Message { get; init; }

    /// <para>A Primitive or Structured value that contains additional information about the error.</para>
    /// <para>This may be omitted.</para>
    /// <para>The value of this member is defined by the Server (e.g. detailed error information, nested errors etc.).</para>
    public JsonNode? Data { get; init; }
}

[JsonSourceGenerationOptions(
    UseStringEnumConverter = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(JsonRpcResponse))]
[JsonSerializable(typeof(List<JsonRpcResponse>))]
[JsonSerializable(typeof(ResultStatus))]
public partial class JsonRpcResponseSerializerContext : JsonSerializerContext;