using System;
using System.Collections.Immutable;
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

    public static bool BoolArg(AttributeData a, int i) => 
        a.ConstructorArguments.Length > i && a.ConstructorArguments[i].Value is bool and true;
    public static int IntArg(AttributeData a, int i) => 
        a.ConstructorArguments.Length > i && a.ConstructorArguments[i].Value is { } v ? Convert.ToInt32(v) : 0;
    public static float FloatArg(AttributeData a, int i) => 
        a.ConstructorArguments.Length > i && a.ConstructorArguments[i].Value is { } v ? Convert.ToSingle(v) : 0f;
    public static string StringArg(AttributeData a, int i) => 
        a.ConstructorArguments.Length > i ? a.ConstructorArguments[i].Value as string ?? "" : "";
    public static bool[] BoolArrayArg(AttributeData a, int i) => 
        ArrayValues(a, i).Select(v => v.Value is bool and true).ToArray();
    public static int[] IntArrayArg(AttributeData a, int i) => 
        ArrayValues(a, i).Select(v => v.Value is { } x ? Convert.ToInt32(x) : 0).ToArray();
    public static float[] FloatArrayArg(AttributeData a, int i) => 
        ArrayValues(a, i).Select(v => v.Value is { } x ? Convert.ToSingle(x) : 0f).ToArray();
    public static string[] StringArrayArg(AttributeData a, int i) => 
        ArrayValues(a, i).Select(v => v.Value as string ?? "").ToArray();
    
    /// <summary>
    /// Will extract an array of arguments if they exist. 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="i"></param>
    /// <returns>An array of elements or an empty array if it fails.</returns>
    private static ImmutableArray<TypedConstant> ArrayValues(AttributeData a, int i) =>
        a.ConstructorArguments.Length > i && a.ConstructorArguments[i].Kind == TypedConstantKind.Array
            ? a.ConstructorArguments[i].Values
            : ImmutableArray<TypedConstant>.Empty;
}