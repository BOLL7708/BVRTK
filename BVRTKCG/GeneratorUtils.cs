using System.Linq;
using Microsoft.CodeAnalysis;

namespace BVRTKCG;

public static class GeneratorUtils
{
    public static string? GetPropName(IFieldSymbol field)
    {
        return field.Name.TrimStart('_') is { Length: >= 1 } s
            ? char.ToUpper(s[0]) + s.Substring(1)
            : null;
    }

    public static string? GetFieldName(IPropertySymbol property)
    {
        return '_' + (property.Name is { Length: >= 1 } s
                ? char.ToLower(s[0]) + s.Substring(1)
                : null
            );
    }
}

public static class GuiElementFactory
{
    public static GuiElement FromField(IFieldSymbol field, GuiGenerator.GuiElementKind kind)
    {
        var element = new GuiElement
        {
            Kind = kind,
            Namespace = field.ContainingType.ContainingNamespace.ToDisplayString(),
            ClassName = field.ContainingType.Name,
            FieldName = field.Name,
            PropName = GeneratorUtils.GetPropName(field) ?? field.Name,
            TypeName = field.Type.ToDisplayString(),
            Order = field.DeclaringSyntaxReferences.FirstOrDefault()?.Span.Start ?? 0
        };

        return element;
    }

    public static GuiElement FromProperty(IPropertySymbol property, GuiGenerator.GuiElementKind kind)
    {
        var element = new GuiElement
        {
            Kind = kind,
            Namespace = property.ContainingType.ContainingNamespace.ToDisplayString(),
            ClassName = property.ContainingType.Name,
            FieldName = GeneratorUtils.GetFieldName(property) ?? property.Name,
            PropName = property.Name,
            TypeName = property.Type.ToDisplayString(),
            Order = property.DeclaringSyntaxReferences.FirstOrDefault()?.Span.Start ?? 0
        };

        return element;
    }
}

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
    
    // Debug
    public string DebugValuePath = "";
}