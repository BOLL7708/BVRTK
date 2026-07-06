using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using BVRTK.Data.Setting;
using Software.Boll.EasyUtils;

namespace BVRTK.Data;

public class Settings
{
    public static readonly Settings Current = new();
    private static readonly Settings Defaults = new();
    private const string Dir = "_settings";

    // TODO: Add more as needed
    public Application Application = new();
    public Server Server = new();

    private static readonly Dictionary<Type, ISettingEntry> SettingsList = new()
    {
        {
            typeof(Application),
            new SettingEntry<Application>(
                () => Current.Application,
                () => Defaults.Application,
                it => Current.Application = it,
                SettingsJsonSerializerContext.Default.Application
            )
        },
        {
            typeof(Server),
            new SettingEntry<Server>(
                () => Current.Server,
                () => Defaults.Server,
                it => Current.Server = it,
                SettingsJsonSerializerContext.Default.Server
            )
        }
    };

    public static void WriteToDisk()
    {
        foreach (var pair in SettingsList)
        {
            WriteFileIfDirty(pair.Value);
        }
    }

    public static void ReadFromDisk()
    {
        foreach (var pair in SettingsList)
        {
            ReadFileIfExists(pair.Value);
        }
    }

    /**
     * 
     */
    public static bool ResetToDefaults(Type type)
    {
        if (!SettingsList.ContainsKey(type)) return false;

        SettingsList.TryGetValue(type, out var entry);
        if (entry == null) return false;

        entry.Reset();
        return true;
    }

    private static string GetFilePath(ISettingEntry entry)
    {
        return $"{Dir}{Path.DirectorySeparatorChar}settings_{entry.Value.__getName()}.json";
    }

    private static void WriteFileIfDirty(ISettingEntry entry)
    {
        FileUtils.EnsureDirectoryExists(Dir);
        if (entry.Value.InternalDirty)
        {
            var filepath = GetFilePath(entry);
            var result = FileUtils.WriteText(filepath, entry.Serialize());
            Console.WriteLine($"Wrote {filepath} ({result.CharsWritten})");
            // TODO: Log problems
            entry.Value.InternalDirty = false;
        }
        else Console.WriteLine($"Did NOT write {entry.Value}");
    }

    private static void ReadFileIfExists(ISettingEntry entry)
    {
        var filepath = GetFilePath(entry);
        var result = FileUtils.ReadText(filepath);
        if (result is { Success: true, CharsRead: > 0 })
        {
            entry.Deserialize(result.Text ?? string.Empty);
        }
        else
        {
            Console.WriteLine($"Failed to write settings file {entry.Value.__getName()}: {result.Exception}");
            // TODO: Log problems
        }
    }
}

internal interface ISettingEntry
{
    AbstractSetting Value { get; }
    void Deserialize(string json);
    string Serialize();
    void Reset();
}

internal class SettingEntry<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(
    Func<T> getValue,
    Func<T> getDefault,
    Action<T> setValue,
    JsonTypeInfo<T> typeInfo) : ISettingEntry where T : AbstractSetting, new()
{
    public AbstractSetting Value => getValue();

    public void Deserialize(string json)
    {
        T? value = null;
        try
        {
            value = JsonSerializer.Deserialize(json, typeInfo);
            Console.WriteLine($"Deserialized: {json} to {value}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to read from disk: {e.Message}");
        }

        if (value != null)
        {
            setValue(value);
        }
        else Console.WriteLine("Failed to deserialize from disk.");
    }

    public string Serialize()
    {
        try
        {
            return JsonSerializer.Serialize(getValue(), typeInfo);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to serialize {getValue()}: {e.Message}");
        }

        return string.Empty;
    }

    public void Reset()
    {
        var current = getValue();
        var defaults = getDefault();
        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            field.SetValue(current, field.GetValue(defaults));
        }
    }
}