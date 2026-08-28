using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

    /// <summary>
    /// Parse out generic types of a collection that has two generics (key, value)
    /// </summary>
    /// <param name="collectionType"></param>
    /// <returns></returns>
    public static KeyValuePair<string, string>? GetTypeGenericPair(string collectionType)
    {
        var start = collectionType.IndexOf('<');
        var end = collectionType.IndexOf('>');
        if (start < 0 || end <= start) return null;

        var types = collectionType.Substring(start + 1, end - start - 1);
        var topCommaIndex = -1;
        var tagDepth = 0;
        for (var i = 0; i < types.Length; i++)
        {
            var c = types[i];
            switch (c)
            {
                case '<':
                    tagDepth++;
                    break;
                case '>':
                    tagDepth--;
                    break;
                case ',' when tagDepth == 0:
                    topCommaIndex = i;
                    break;
            }
        }

        if (topCommaIndex < 0) return null;

        var key = types.Substring(0, topCommaIndex).Trim();
        var value = types.Substring(topCommaIndex + 1).Trim();
        return new KeyValuePair<string, string>(key, value);
    }

    #region Arguments

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

    #endregion
    
        
    #region Formatting

    public static string ArrayToString(string[] array)
    {
        return "["+string.Join(", ", array.Select<string, object>(s => 
            SymbolDisplay.FormatLiteral(s, quote:true))
        )+"]";
    }    
    #endregion
}