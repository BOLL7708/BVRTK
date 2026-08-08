using System;

namespace BVRTKCG.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class GuiCheckboxAttribute(string label, string tooltip): Attribute
{
    public string Label = label;
    public string Tooltip = tooltip;
}

[AttributeUsage(AttributeTargets.Field)]
public class GuiSliderAttribute(string label, string tooltip, float min, float max, float step, float start): Attribute
{
    public string Label = label;
    public string Tooltip = tooltip;
    public float Min = min;
    public float Max = max;
    public float Step = step;
    public float Start = start;
}

[AttributeUsage(AttributeTargets.Field)]
public class GuiTextAttribute(string label, string tooltip): Attribute
{
    public string Label = label;
    public string Tooltip = tooltip;
}

[AttributeUsage(AttributeTargets.Property)]
public class GuiDebugAttribute(string valuePath): Attribute
{
    public string ValuePath = valuePath;
}


