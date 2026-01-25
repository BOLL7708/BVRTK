using System.Text.Json.Serialization.Metadata;
using BVRTK.Components.Server;
using static System.Text.Json.JsonSerializer;

namespace BVRTK.Utils;

/**
 * Derived from: https://github.com/BOLL7708/EasyFramework/blob/main/JsonUtils.cs
 */
public static class JsonUtils
{
    public record JsonDataParseResult<T>(T? Data, T Empty, string Message);

    public static JsonDataParseResult<T> ParseData<T>(string? dataStr) where T : class, new()
    {
        dataStr ??= "";
        T? data = null;
        var errorMessage = "";
        try
        {
            var typeInfo = (JsonTypeInfo<T>?)AppJsonSerializerContext.Default.GetTypeInfo(typeof(T));
            if (typeInfo != null)
            {
                data = Deserialize(dataStr, typeInfo);
            }
        }
        catch (Exception e)
        {
            errorMessage = e.Message;
        }

        return new JsonDataParseResult<T>(data, new T(), errorMessage);
    }

    public static string SerializeData<T>(T? data) where T : class
    {
        var typeInfo = (JsonTypeInfo<T?>?)AppJsonSerializerContext.Default.GetTypeInfo(typeof(T));
        return typeInfo != null ? Serialize(data, typeInfo) : "";
    }
}