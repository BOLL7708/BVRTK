using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BVRTKCG;

[Generator]
public class DirtyTrackingGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        #region Attribute Registration

        context.RegisterPostInitializationOutput(i =>
        {
            const string attributeSource = """
                                             namespace BVRTK;
                                             public class TrackDirtyAttribute: System.Attribute {} 
                                           """;
            i.AddSource($"BVRTK.Data.Setting.TrackDirtyAttribute.g.cs", attributeSource);
        });

        #endregion

        var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BVRTK.TrackDirtyAttribute",
            predicate: (node, _) => node is ClassDeclarationSyntax,
            transform: (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
        ).Where(m => m is not null);

        context.RegisterSourceOutput(classes, (ctx, classSymbol) =>
            {
                var fields = classSymbol
                    .GetMembers()
                    .OfType<IFieldSymbol>();

                var sb = new StringBuilder();
                sb.AppendLine("namespace BVRTK.Data.Setting;");
                sb.AppendLine("using System.Text.Json.Serialization;");
                sb.AppendLine($"public partial class {classSymbol.Name} : AbstractSetting");
                sb.AppendLine("{");

                foreach (var field in fields)
                {
                    var propName = field.Name.TrimStart('_') is { Length: >= 1 } s ? char.ToUpper(s[0]) + s.Substring(1) : null;
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
                                                    {{field.Name}} = value;
                                                    InternalDirty = true;
                                                    Console.WriteLine($"Settings: Updated {{classSymbol.Name}}.{{field.Name}} to {value} and marked as DIRTY!");
                                                }
                                                // TODO: Add log handler here to report failure to set.
                                            }
                                        }
                                    """);
                }

                sb.AppendLine("}");
                ctx.AddSource($"{classSymbol.ContainingNamespace}.{classSymbol.Name}.g.cs", sb.ToString());
            }
        );
    }
}