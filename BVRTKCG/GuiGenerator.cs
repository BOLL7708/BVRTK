using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BVRTKCG.Attributes;
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

        #region Inputs

        Checkbox,
        FloatSlider,
        IntSlider,
        Int,
        IntModal,
        Text,
        Combo,

        #endregion

        #region Text

        Title,
        Label,

        #endregion

        SameLine,
        Debug,
        Test,
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

        var floatSliders = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BVRTKCG.Attributes.GuiFloatSliderAttribute",
            static (n, _) => n is VariableDeclaratorSyntax,
            static (ctx, _) =>
            {
                var e = GuiElementFactory.FromField((IFieldSymbol)ctx.TargetSymbol, GuiElementKind.FloatSlider);
                var a = ctx.Attributes[0];
                e.Label = StringArg(a, 0);
                e.Tooltip = StringArg(a, 1);
                e.FloatSliderMin = FloatArg(a, 2);
                e.FloatSliderMax = FloatArg(a, 3);
                e.FloatSliderFormat = StringArg(a, 4);
                return e;
            }
        );
        
        var intSliders = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BVRTKCG.Attributes.GuiIntSliderAttribute",
            static (n, _) => n is VariableDeclaratorSyntax,
            static (ctx, _) =>
            {
                var e = GuiElementFactory.FromField((IFieldSymbol)ctx.TargetSymbol, GuiElementKind.IntSlider);
                var a = ctx.Attributes[0];
                e.Label = StringArg(a, 0);
                e.Tooltip = StringArg(a, 1);
                e.IntSliderMin = IntArg(a, 2);
                e.IntSliderMax = IntArg(a, 3);
                return e;
            }
        );

        var ints = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BVRTKCG.Attributes.GuiIntAttribute",
            static (n, _) => n is VariableDeclaratorSyntax,
            static (ctx, _) =>
            {
                var e = GuiElementFactory.FromField((IFieldSymbol)ctx.TargetSymbol, GuiElementKind.Int);
                var a = ctx.Attributes[0];
                e.Label = StringArg(a, 0);
                e.Tooltip = StringArg(a, 1);
                e.IntWidth = FloatArg(a, 2);
                e.IntStep = IntArg(a, 3);
                return e;
            }
        );
        
        var intModals = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BVRTKCG.Attributes.GuiIntModalAttribute",
            static (n, _) => n is VariableDeclaratorSyntax,
            static (ctx, _) =>
            {
                var e = GuiElementFactory.FromField((IFieldSymbol)ctx.TargetSymbol, GuiElementKind.IntModal);
                var a = ctx.Attributes[0];
                e.Label = StringArg(a, 0);
                e.Tooltip = StringArg(a, 1);
                e.IntWidth = FloatArg(a, 2);
                e.IntStep = IntArg(a, 3);
                e.IntModalTitle = StringArg(a, 4);
                return e;
            }
        );

        var combos = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BVRTKCG.Attributes.GuiComboAttribute",
            static (n, _) => n is VariableDeclaratorSyntax,
            static (ctx, _) =>
            {
                var e = GuiElementFactory.FromField((IFieldSymbol)ctx.TargetSymbol, GuiElementKind.Combo);
                var a = ctx.Attributes[0];
                e.Label = StringArg(a, 0);
                e.Tooltip = StringArg(a, 1);
                e.ComboWidth = FloatArg(a, 2);
                e.ComboValuesConstantPath = StringArg(a, 3);
                return e;
            }
        );

        // var texts = [];

        var titles = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BVRTKCG.Attributes.GuiTitleAttribute",
            static (n, _) => n is VariableDeclaratorSyntax,
            static (ctx, _) =>
            {
                var e = GuiElementFactory.FromField((IFieldSymbol)ctx.TargetSymbol, GuiElementKind.Title);
                var a = ctx.Attributes[0];
                e.Label = StringArg(a, 0);
                e.Tooltip = StringArg(a, 1);
                return e;
            }
        );

        var labels = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BVRTKCG.Attributes.GuiLabelAttribute",
            static (n, _) => n is VariableDeclaratorSyntax,
            static (ctx, _) =>
            {
                var e = GuiElementFactory.FromField((IFieldSymbol)ctx.TargetSymbol, GuiElementKind.Label);
                var a = ctx.Attributes[0];
                e.Label = StringArg(a, 0);
                e.SameLine = BoolArg(a, 1);
                return e;
            }
        );

        var sameLines = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BVRTKCG.Attributes.GuiSameLine",
            static (n, _) => n is VariableDeclaratorSyntax, static (ctx, _)
                => GuiElementFactory.FromField((IFieldSymbol)ctx.TargetSymbol, GuiElementKind.SameLine));

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
            titles.Collect(),
            labels.Collect(),
            checkboxes.Collect(),
            floatSliders.Collect(),
            intSliders.Collect(),
            ints.Collect(),
            intModals.Collect(),
            combos.Collect(),
            sameLines.Collect(),
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
            sb.AppendLine("using BVRTK;");
            sb.AppendLine("using BVRTK.Data;");
            sb.AppendLine("using BVRTK.Components.Graphics;");
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
                        break;
                    case GuiElementKind.FloatSlider:
                        sb.AppendLine($"""
                                               var {e.FieldName} = Settings.Current.{e.ClassName}.{e.PropName};
                                               if (ImGui.SliderFloat("{e.Label}", ref {e.FieldName}, {e.FloatSliderMin}f, {e.FloatSliderMax}f, "{e.FloatSliderFormat}")) Settings.Current.{e.ClassName}.{e.PropName} = {e.FieldName};
                                       """);
                        break;                    
                    case GuiElementKind.IntSlider:
                        sb.AppendLine($"""
                                               var {e.FieldName} = Settings.Current.{e.ClassName}.{e.PropName};
                                               if (ImGui.SliderInt("{e.Label}", ref {e.FieldName}, {e.IntSliderMin}, {e.IntSliderMax})) Settings.Current.{e.ClassName}.{e.PropName} = {e.FieldName};
                                       """);
                        break;
                    case GuiElementKind.Int:
                        sb.AppendLine($"""
                                               var {e.FieldName} = Settings.Current.{e.ClassName}.{e.PropName};
                                               ImGui.SetNextItemWidth({e.IntWidth}f*Constants.OverlayGuiScale);
                                               if (ImGui.InputInt("{e.Label}", ref {e.FieldName}, {e.IntStep}, ImGuiInputTextFlags.CharsDecimal)) Settings.Current.{e.ClassName}.{e.PropName} = {e.FieldName};
                                       """);
                        break;
                    case GuiElementKind.IntModal:
                        sb.AppendLine($"""
                                               GuiUtils.OpenModalForInt(
                                                   "{e.IntModalTitle}##{e.ClassName}.{e.PropName}.{e.IntModalTitle}", 
                                                   "{e.Label}", 
                                                   "{e.IntModalTitle}", 
                                                   {e.IntWidth}f,
                                                   Settings.Current.{e.ClassName}.{e.PropName}
                                               );
                                               GuiUtils.DrawModalForInt(
                                                   "{e.IntModalTitle}##{e.ClassName}.{e.PropName}.{e.IntModalTitle}", 
                                                   "{e.Label}", 
                                                   {e.IntWidth}f,
                                                   Settings.Current.{e.ClassName}.{e.PropName},
                                                   value => Settings.Current.{e.ClassName}.{e.PropName} = value
                                               );
                                       """);
                        break;
                    case GuiElementKind.Combo:
                        sb.AppendLine($$"""
                                               var {{e.FieldName}} = Settings.Current.{{e.ClassName}}.{{e.PropName}};
                                               var {{e.FieldName}}Index = GuiUtils.GetIndexOfTagInLabels({{e.ComboValuesConstantPath}}, {{e.FieldName}});
                                               ImGui.SetNextItemWidth({{e.ComboWidth}}f * Constants.OverlayGuiScale);
                                               if(ImGui.Combo("{{e.Label}}", ref {{e.FieldName}}Index, {{e.ComboValuesConstantPath}}, {{e.ComboValuesConstantPath}}.Length)) 
                                               {
                                                   var nameStr = {{e.ComboValuesConstantPath}}[{{e.FieldName}}Index];
                                                   Settings.Current.{{e.ClassName}}.{{e.PropName}} = GuiUtils.GetTagFromLabel(nameStr);
                                               };
                                       """);
                        break;
                    case GuiElementKind.Text:
                        sb.AppendLine($"""
                                        

                                       """);
                        
                        // TODO: Implement
                        break;
                    case GuiElementKind.Title:
                        sb.AppendLine($"""
                                               ImGui.Dummy(Vector2.Zero);
                                               GuiUtils.DrawTitle("{e.Label}");
                                       """);
                        break;
                    case GuiElementKind.Label:
                        sb.AppendLine($"""
                                               ImGui.AlignTextToFramePadding();
                                               ImGui.TextUnformatted("{e.Label}");
                                       """);
                        if (e.SameLine) sb.AppendLine("        ImGui.SameLine();");
                        break;
                    case GuiElementKind.SameLine:
                        sb.AppendLine("        ImGui.SameLine();");
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

                AppendTooltip(sb, e.Tooltip);
                sb.AppendLine();
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            ctx.AddSource($"{group.Key.Namespace}.{group.Key.ClassName}.GuiRenderer.g.cs", sb.ToString());
        }
    }

    private static void AppendTooltip(StringBuilder sb, string tooltip)
    {
        if (tooltip.Trim().Length == 0) return;
        sb.AppendLine($$"""
                                GuiUtils.DrawTooltip("{{tooltip}}");
                        """);
    }
}