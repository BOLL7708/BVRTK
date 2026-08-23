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

[AttributeUsage(AttributeTargets.Field)]
public class GuiIntAttribute(string label, string tooltip, float width, int step) : Attribute
{
    public string Label = label;
    public string Tooltip = tooltip;
    public float Width = width;
    public int Step = step;
}

[AttributeUsage(AttributeTargets.Property)]
public class GuiDebugAttribute(string valuePath): Attribute
{
    public string ValuePath = valuePath;
}

[AttributeUsage(AttributeTargets.Property)]
public class GuiTestAttribute(bool b, int i, float f, string s, bool[] ba, int[] ia, float[] fa, string[] sa): Attribute
{
    public bool B = b;
    public int I = i;
    public float F = f;
    public string S = s;
    public bool[] Ba = ba;
    public int[] Ia = ia;
    public float[] Fa = fa;
    public string[] Sa = sa;
}

