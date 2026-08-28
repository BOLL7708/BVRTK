namespace BVRTKCG;

public class GuiElement
{
    // Base
    public GuiGenerator.GuiElementKind Kind = GuiGenerator.GuiElementKind.Unknown;
    public string Namespace = "";
    public string ClassName = "";
    public string FieldName = "";
    public string PropName = "";
    public string TypeName = "";
    public int Order = 0;

    // Universal
    public string Label = "";
    public string Tooltip = "";

    // Specific
    // Slider
    public float FloatSliderMin = 0;
    public float FloatSliderMax = 0;
    public string FloatSliderFormat = "";
    public int IntSliderMin = 0;
    public int IntSliderMax = 0;
    
    // Int
    public float IntWidth = 0;
    public int IntStep = 0;
    public string IntModalTitle = "";
    
    // Combo
    public float ComboWidth = 0;
    public string ComboValuesConstantPath = "";
    
    // General
    public bool SameLine = false;
    
    // Debug
    public string DebugValuePath = "";
    public string TestLog = "";
}