using System.Text.Json.Serialization;

namespace BVRTK.Data.Setting;

public class AbstractSetting
{
    public string InternalName()
    {
        return GetType().Name.ToLower();  
    }

    [JsonIgnore]
    public bool InternalDirty { get; set; }
}