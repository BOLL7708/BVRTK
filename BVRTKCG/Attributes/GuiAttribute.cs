using System;

namespace BVRTKCG.Attributes;

#region Input

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class GuiCheckboxAttribute(string label, string tooltip) : Attribute
{
    public string Label = label;
    public string Tooltip = tooltip;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class GuiFloatSliderAttribute(string label, string tooltip, float min, float max, string format) : Attribute
{
    public string Label = label;
    public string Tooltip = tooltip;
    public float Min = min;
    public float Max = max;
    public string Format = format;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class GuiIntSliderAttribute(string label, string tooltip, int min, int max) : Attribute
{
    public string Label = label;
    public string Tooltip = tooltip;
    public int Min = min;
    public int Max = max;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class GuiIntAttribute(string label, string tooltip, float width, int step) : Attribute
{
    public string Label = label;
    public string Tooltip = tooltip;
    public float Width = width;
    public int Step = step;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class GuiIntModalAttribute(string label, string tooltip, float width, int step, string modalTitle) : Attribute
{
    public string Label = label;
    public string Tooltip = tooltip;
    public float Width = width;
    public int Step = step;
    public string ModalTitle = modalTitle;
}

/// <summary>
/// 
/// </summary>
/// <param name="label"></param>
/// <param name="tooltip"></param>
/// <param name="width"></param>
/// <param name="valuesConstantPath">The path to a readonly value that represents a string array.</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class GuiComboAttribute(string label, string tooltip, float width, string valuesConstantPath) : Attribute
{
    public string Label = label;
    public string Tooltip = tooltip;
    public float Width = width;
    public string ValuesConstantPath = valuesConstantPath;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class GuiTextAttribute(string label, string tooltip) : Attribute
{
    public string Label = label;
    public string Tooltip = tooltip;
}

#endregion

#region Text

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class GuiTitleAttribute(string label, string tooltip) : Attribute
{
    public string Label = label;
    public string Tooltip = tooltip;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class GuiLabelAttribute(string label, bool sameLine) : Attribute
{
    public string Label = label;
    public bool SameLine = sameLine;
}

#endregion

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class GuiSameLine() : Attribute
{
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class GuiDebugAttribute(string valuePath) : Attribute
{
    public string ValuePath = valuePath;
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class GuiTestAttribute(bool b, int i, float f, string s, bool[] ba, int[] ia, float[] fa, string[] sa) : Attribute
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