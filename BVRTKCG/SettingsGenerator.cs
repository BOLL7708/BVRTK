using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BVRTKCG;

[Generator]
public class SettingsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BVRTKCG.Attributes.SettingAttribute",
            predicate: (node, _) => node is ClassDeclarationSyntax,
            transform: (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
        ).Where(m => m is not null);

        context.RegisterSourceOutput(classes, (ctx, classSymbol) =>
            {
                var fields = classSymbol
                    .GetMembers()
                    .OfType<IFieldSymbol>()
                    .Where(f => !f.IsImplicitlyDeclared); // Will avoid auto-created backing fields so we can discretely use private properties as non-settings GUI generators.
                var fieldsArr = fields.ToArray();
                GenerateSettingsHandlers(ctx, classSymbol, fieldsArr);
                GenerateSettingsProps(ctx, classSymbol, fieldsArr);
            }
        );
    }

    private void GenerateSettingsHandlers(SourceProductionContext ctx, INamedTypeSymbol classSymbol, IFieldSymbol[] fields)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace BVRTK.Data;");
        sb.AppendLine($"public static partial class SettingsChangeHandlers");
        sb.AppendLine("{");
        foreach (var field in fields)
        {
            var propName = GeneratorUtils.GetPropName(field);
            if (propName == null) continue;

            var typeName = field.Type.ToDisplayString();
            // TODO: Add log handler 
            sb.AppendLine($$"""
                                #nullable enable
                                public static event ValueChangeHandler<{{typeName}}>? On{{classSymbol.Name}}{{propName}}Changed;
                                internal static void Notify{{classSymbol.Name}}{{propName}}Changed({{typeName}} current, {{typeName}} previous) => On{{classSymbol.Name}}{{propName}}Changed?.Invoke(current, previous);  
                            """);
        }

        sb.AppendLine("}");
        ctx.AddSource($"{classSymbol.ContainingNamespace}.SettingsChangeHandler.{classSymbol.Name}.g.cs", sb.ToString());
    }

    private void GenerateSettingsProps(SourceProductionContext ctx, INamedTypeSymbol classSymbol, IFieldSymbol[] fields)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace BVRTK.Data.Setting;");
        sb.AppendLine("using System.Text.Json.Serialization;");
        sb.AppendLine($"public partial class {classSymbol.Name} : AbstractSetting");
        sb.AppendLine("{");
        foreach (var field in fields)
        {
            var propName = GeneratorUtils.GetPropName(field);
            if (propName == null) continue;

            var typeName = field.Type.ToDisplayString();
            // TODO: Add log handler 
            sb.AppendLine($$"""
                                public partial {{typeName}} {{propName}}
                                {
                                     get => {{field.Name}};
                                     set
                                     {
                                         if (!EqualityComparer<{{typeName}}>.Default.Equals({{field.Name}}, value)) 
                                         {
                                             Data.SettingsChangeHandlers.Notify{{classSymbol.Name}}{{propName}}Changed(value, {{field.Name}});
                                             {{field.Name}} = value;
                                             InternalDirty = true;
                                         }
                                         // TODO: Add log handler here to report failure to set.
                                     }
                                 }
                            """);
        }

        sb.AppendLine("}");
        ctx.AddSource($"{classSymbol.ContainingNamespace}.{classSymbol.Name}.g.cs", sb.ToString());
    }
}