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
                    .OfType<IPropertySymbol>();
                var fieldsArr = fields.ToArray();
                GenerateSettingsHandlers(ctx, classSymbol, fieldsArr);
                GenerateSettingsProps(ctx, classSymbol, fieldsArr);
            }
        );
    }

    private void GenerateSettingsHandlers(SourceProductionContext ctx, INamedTypeSymbol classSymbol, IPropertySymbol[] properties)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace BVRTK.Data;");
        sb.AppendLine($"public static partial class SettingsChangeHandlers");
        sb.AppendLine("{");
        foreach (var prop in properties)
        {
            var fieldName = GeneratorUtils.GetFieldName(prop);
            if (fieldName == null) continue;

            var typeName = prop.Type.ToDisplayString();
            // TODO: Add log handler

            sb.AppendLine($$"""
                                #nullable enable
                                public static event ValueChangeHandler<{{typeName}}>? On{{classSymbol.Name}}{{prop.Name}}Changed;
                                internal static void Notify{{classSymbol.Name}}{{prop.Name}}Changed({{typeName}} newValue) => On{{classSymbol.Name}}{{prop.Name}}Changed?.Invoke(newValue);  
                            """);
        }

        sb.AppendLine("}");
        ctx.AddSource($"{classSymbol.ContainingNamespace}.SettingsChangeHandler.{classSymbol.Name}.g.cs", sb.ToString());
    }

    private void GenerateSettingsProps(SourceProductionContext ctx, INamedTypeSymbol classSymbol, IPropertySymbol[] properties)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace BVRTK.Data.Setting;");
        sb.AppendLine("using System.Text.Json.Serialization;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine($"public partial class {classSymbol.Name} : AbstractSetting");
        sb.AppendLine("{");
        foreach (var prop in properties)
        {
            var fieldName = GeneratorUtils.GetFieldName(prop);
            if (fieldName == null) continue;

            var typeName = prop.Type.ToDisplayString();
            // TODO: Add log handler 

//             sb.AppendLine($$"""
//                                 private {{typeName}} {{fieldName}} = {{GetDefaultFor(prop, typeName)}}; 
//                             """);

            if (typeName.Contains("ImmutableDictionary"))
            {
                var typePair = GeneratorUtils.GetTypeGenericPair(typeName);
                if (typePair == null) continue;

                var keyType = typePair.Value.Key;
                var valueType = typePair.Value.Value;
                sb.AppendLine($$"""
                                    /// Generated Dictionary Setter for {{prop.Name}}
                                    internal void SetIn{{prop.Name}}({{keyType}} key, {{valueType}} value)
                                    {
                                        if (!{{prop.Name}}.TryGetValue(key, out var existing)
                                            || !EqualityComparer<{{valueType}}>.Default.Equals(existing, value))
                                        {
                                            {{prop.Name}} = {{prop.Name}}.SetItem(key, value);
                                            InternalDirty = true;
                                            Data.SettingsChangeHandlers.Notify{{classSymbol.Name}}{{prop.Name}}Changed({{prop.Name}});
                                            Console.WriteLine("Key [{{keyType}}] in [{{typeName}}] {{prop.Name}} updated with [{{valueType}}], dirty state set.");
                                        }
                                     }
                                     
                                     /// Generated Dictionary Remover for {{prop.Name}}
                                     internal void RemoveFrom{{prop.Name}}({{keyType}} key)
                                     {
                                         if ({{prop.Name}}.TryGetValue(key, out var existing))
                                         {
                                             {{prop.Name}} = {{prop.Name}}.Remove(key);
                                             InternalDirty = true;
                                             Data.SettingsChangeHandlers.Notify{{classSymbol.Name}}{{prop.Name}}Changed({{prop.Name}});
                                             Console.WriteLine("Key [{{keyType}}] in [{{typeName}}] {{prop.Name}} updated with [{{valueType}}], dirty state set.");
                                         }
                                      }
                                """);
            }
            else if (typeName.Contains("ImmutableList"))
            {
                var valueType = GeneratorUtils.GetTypeSingleGeneric(typeName);
                if (valueType == null) continue;

                sb.AppendLine($$"""
                                    /// Generated List Adder for {{prop.Name}}, will only add if new.
                                    internal void AddIfNewTo{{prop.Name}}({{valueType}} value)
                                    {
                                        if(!{{prop.Name}}.Contains(value)) AddTo{{prop.Name}}(value);
                                    }
                                    
                                    /// Generated List Adder for {{prop.Name}}.
                                    internal void AddTo{{prop.Name}}({{valueType}} value)
                                    {
                                        {{prop.Name}} = {{prop.Name}}.Add(value);
                                        InternalDirty = true;
                                        Data.SettingsChangeHandlers.Notify{{classSymbol.Name}}{{prop.Name}}Changed({{prop.Name}});
                                        Console.WriteLine($"[{{typeName}}] {{prop.Name}} item added, dirty state set.");
                                    }
                                    
                                    /// Generated List Remover for {{prop.Name}}.
                                    internal void RemoveFrom{{prop.Name}}({{valueType}} value)
                                    {
                                        if(!{{prop.Name}}.Contains(value)) return;
                                        
                                        {{prop.Name}} = {{prop.Name}}.Remove(value);
                                        InternalDirty = true;
                                        Data.SettingsChangeHandlers.Notify{{classSymbol.Name}}{{prop.Name}}Changed({{prop.Name}});
                                        Console.WriteLine($"[{{typeName}}] {{prop.Name}} item removed, dirty state set.");
                                    }
                                    
                                    /// Generated List Replaced for {{prop.Name}}. Nulling an argument will make for add or remove.
                                    #nullable enable
                                    internal void ReplaceIn{{prop.Name}}({{valueType}}? currentValue, {{valueType}}? newValue)
                                    {
                                        if(currentValue == null && newValue == null) return;
                                        
                                        if(currentValue != null && newValue != null) {
                                            var index = {{prop.Name}}.IndexOf(currentValue);
                                            if(index >= 0) {
                                                {{prop.Name}} = {{prop.Name}}.SetItem(index, newValue);
                                            }
                                        }
                                        if(currentValue != null && newValue == null) {
                                            {{prop.Name}} = {{prop.Name}}.Remove(currentValue);
                                        }
                                        if(currentValue == null && newValue != null) {
                                            {{prop.Name}} = {{prop.Name}}.Add(newValue);
                                        }
                                        
                                        InternalDirty = true;
                                        Data.SettingsChangeHandlers.Notify{{classSymbol.Name}}{{prop.Name}}Changed({{prop.Name}});
                                        Console.WriteLine($"[{{typeName}}] {{prop.Name}} item replaced, dirty state set.");
                                    }               
                                    #nullable disable
                                """);
            }
            else
            {
                sb.AppendLine($$"""
                                    /// Generated Value Setter for {{prop.Name}}
                                    internal void Set{{prop.Name}}({{typeName}} value)
                                    {
                                        if (!EqualityComparer<{{typeName}}>.Default.Equals({{prop.Name}}, value)) 
                                        {
                                            {{prop.Name}} = value;
                                            InternalDirty = true;
                                            Data.SettingsChangeHandlers.Notify{{classSymbol.Name}}{{prop.Name}}Changed({{prop.Name}});
                                            Console.WriteLine("[{{typeName}}] {{prop.Name}} updated, dirty state set.");
                                        }
                                        // TODO: Add log handler here to report failure to set.
                                    }
                                """);
            }
        }

        sb.AppendLine("}");
        ctx.AddSource($"{classSymbol.ContainingNamespace}.{classSymbol.Name}.g.cs", sb.ToString());
    }
}