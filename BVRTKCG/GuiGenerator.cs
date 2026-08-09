using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static BVRTKCG.GeneratorUtils;

namespace BVRTKCG;

[Generator]
public class GuiGenerator : IIncrementalGenerator
{
    public enum GuiElementKind
    {
        Unknown,
        Checkbox,
        Slider,
        Text,
        Debug,
        Test
    }

    

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var checkboxes = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BVRTKCG.Attributes.GuiCheckboxAttribute",
            static (n, _) => n is VariableDeclaratorSyntax,
            static (ctx, _) =>
            {
                var e = GuiElementFactory.FromField((IFieldSymbol)ctx.TargetSymbol, GuiElementKind.Checkbox);
                var a = ctx.Attributes[0];
                e.Label = StringArg(a, 0);
                e.Tooltip = StringArg(a, 1);
                return e;
            });

        var sliders = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BVRTKCG.Attributes.GuiSliderAttribute",
            static (n, _) => n is VariableDeclaratorSyntax,
            static (ctx, _) =>
            {
                var e = GuiElementFactory.FromField((IFieldSymbol)ctx.TargetSymbol, GuiElementKind.Slider);
                var a = ctx.Attributes[0];
                e.Label = StringArg(a, 0);
                e.Tooltip = StringArg(a, 1);
                e.SliderMin = FloatArg(a, 2);
                e.SliderMax = FloatArg(a, 3);
                e.SliderStep = FloatArg(a, 4);
                e.SliderStart= FloatArg(a, 5);
                return e;
            }
        );

        // var texts = [];

        var debugs = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BVRTKCG.Attributes.GuiDebugAttribute",
            static (n, _) => n is PropertyDeclarationSyntax,
            static (ctx, _) =>
            {
                var e = GuiElementFactory.FromProperty((IPropertySymbol)ctx.TargetSymbol, GuiElementKind.Debug);
                var a = ctx.Attributes[0];
                e.DebugValuePath = StringArg(a, 0);
                return e;
            });

        var tests = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BVRTKCG.Attributes.GuiTestAttribute",
            static (n, _) => n is PropertyDeclarationSyntax,
            static (ctx, _) =>
            {
                var sb = new StringBuilder();
                var e = GuiElementFactory.FromProperty((IPropertySymbol)ctx.TargetSymbol, GuiElementKind.Test);
                var a = ctx.Attributes[0];
                sb.AppendLine($"Bool: {BoolArg(a, 0)}");
                sb.AppendLine($"Int: {IntArg(a, 1)}");
                sb.AppendLine($"Float: {FloatArg(a, 2)}");
                sb.AppendLine($"String: {StringArg(a, 3)}");
                sb.AppendLine($"BoolArray: {string.Join(", ", BoolArrayArg(a, 4))}");
                sb.AppendLine($"IntArray: {string.Join(", ", IntArrayArg(a, 5))}");
                sb.AppendLine($"FloatArray: {string.Join(", ", FloatArrayArg(a, 6))}");
                sb.AppendLine($"StringArray: {string.Join(", ", StringArrayArg(a, 7))}");
                e.TestLog = sb.ToString();
                return e;
            });
        
        var providers = new[]
        {
            checkboxes.Collect(),
            sliders.Collect(),
            debugs.Collect(),
            tests.Collect()
        };
        var all = providers.Aggregate(Merge);

        context.RegisterSourceOutput(all, GenerateGuiRenderer);
    }

    private static IncrementalValueProvider<ImmutableArray<GuiElement>> Merge(
        IncrementalValueProvider<ImmutableArray<GuiElement>> a,
        IncrementalValueProvider<ImmutableArray<GuiElement>> b)
        => a.Combine(b).Select(static (p, _) => p.Left.AddRange(p.Right));

    private static void GenerateGuiRenderer(SourceProductionContext ctx, ImmutableArray<GuiElement> elements)
    {
        foreach (var group in elements.GroupBy(e => (e.Namespace, e.ClassName)))
        {
            var ordered = group.OrderBy(e => e.Order).ToArray();
            
            var sb = new StringBuilder();
            sb.AppendLine($"namespace {group.Key.Namespace};");
            sb.AppendLine("using BVRTKCG.Attributes;");
            sb.AppendLine("using System.Numerics;");
            sb.AppendLine("using BVRTK.Data;");
            sb.AppendLine("using Hexa.NET.ImGui;");
            sb.AppendLine("public partial class GuiRenderers");
            sb.AppendLine("{");
            sb.AppendLine($"    public static void Render{group.Key.ClassName}Page()");
            sb.AppendLine("    {");
            foreach (var e in ordered)
            {
                switch (e.Kind)
                {
                    case GuiElementKind.Checkbox:
                        sb.AppendLine($"""
                                                var {e.FieldName} = Settings.Current.{e.ClassName}.{e.PropName};
                                                if (ImGui.Checkbox("{e.Label}", ref {e.FieldName})) Settings.Current.{e.ClassName}.{e.PropName} = {e.FieldName};
                                        """);
                        AppendTooltip(sb, e.Tooltip);
                        break;
                    case GuiElementKind.Slider:
                        // TODO: Implement
                        break;
                    case GuiElementKind.Debug:
                        sb.AppendLine($$"""
                                               ImGui.TextColored(new Vector4(1f, 0, 0, 1f), $"Debug Value: {{{e.DebugValuePath}}}");
                                       """);
                        break;
                    case GuiElementKind.Test:
                        sb.AppendLine($"ImGui.TextWrapped(\n\"\"\"\n{e.TestLog}\"\"\");");
                        break;
                }
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");
            ctx.AddSource($"{group.Key.Namespace}.{group.Key.ClassName}.GuiRenderer.g.cs", sb.ToString());            
        }
    }

    private static void AppendTooltip(StringBuilder sb, string tooltip)
    {
        if (tooltip.Trim().Length == 0) return;
        sb.AppendLine($"        if (Settings.Current.Application.ShowTooltips && ImGui.IsItemHovered()) ImGui.SetTooltip(\"{tooltip}\");");
    }
}