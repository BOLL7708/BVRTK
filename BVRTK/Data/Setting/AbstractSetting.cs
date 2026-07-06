using System.Text.Json.Serialization;

namespace BVRTK.Data.Setting;

public class AbstractSetting
{
    public string __getName()
    {
        return GetType().Name.ToLower();  
    } 

    [JsonIgnore]
    internal bool InternalDirty = false;

    public void __setDirty()
    {
        InternalDirty = true;
    }
}