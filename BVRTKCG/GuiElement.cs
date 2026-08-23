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
    public float SliderMin = 0;
    public float SliderMax = 0;
    public float SliderStep = 0;
    public float SliderStart = 0;
    
    // Int
    public float IntWidth = 0;
    public int IntStep = 0;
    
    // Debug
    public string DebugValuePath = "";
    public string TestLog = "";
}