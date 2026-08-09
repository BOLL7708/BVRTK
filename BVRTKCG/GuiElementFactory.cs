using System.Linq;
using Microsoft.CodeAnalysis;

namespace BVRTKCG;

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