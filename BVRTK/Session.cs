using System.Collections.Immutable;
using BVRTK.Components.Graphics;
using BVRTK.Components.KeyboardSimulator;
using EasyOpenVR.Data.Manifest;
using Hexa.NET.ImGui;

namespace BVRTK;

public static class Session
{
#if DEBUG
    public const bool isDebug = true;
#else
    public const bool isDebug = false;
#endif

    public static string Version { get; set; } = "v0.0.0";
    public static bool ShowAbout = false;
    public static bool ShowMetrics = false;

    public static unsafe class GuiFonts
    {
        public static ImFont* Regular { get; set; }
        public static ImFont* Bold { get; set; }
        public static ImFont* Italic { get; set; }
        public static ImFont* BoldItalic { get; set; }
    }

    public static class GuiImages
    {
        public static GlImage Logo;
    }

    public static readonly CancellationTokenSource ProgramCts = new();

    public static bool ExitPressed { get; set; }

    public static bool OverlayFocus { get; set; }
    public static bool DesktopFocus { get; set; }


    public static void SetActionEntriesForSet(string actionSet, ActionGuiEntry[] entries)
    {
        ActionEntries = ActionEntries.SetItem(actionSet, entries);
        ActionGuiIds = ActionGuiIds.SetItem(actionSet, [.. entries.Select(entry => entry.Id)]);
    }
    
    /// Filled at action manifest registration, includes all actions for the entire project.
    internal static ImmutableDictionary<string, ActionGuiEntry[]> ActionEntries = [];
    internal static ImmutableDictionary<string, string[]> ActionGuiIds = []; // Full IDs
    
    internal static volatile string[] SupportedLanguageGuiIds = Utils.GetSupportedLanguageGuiIds(); // Full IDs
    internal static ImmutableDictionary<string, string> SupportedLanguageGuiTagToLabel = Utils.GetSupportedLanguageGuiIdPairs().ToImmutableDictionary(); // IDs as pairs
    
    internal static volatile string[] KeyboardSimulatorKeyCodeGuiIds = KeyboardSimulatorUtils.GetGuiIds(); // Full IDs
    internal static ImmutableDictionary<string, string> KeyboardSimulatorKeyCodeGuiTagToLabel = KeyboardSimulatorUtils.GetGuiIdPairs().ToImmutableDictionary(); // IDs as pairs
    internal static volatile string[] KeyboardSimulatorTriggerGuiIds = HardwareInputTrigger.GetTriggerGuiIds; // Full IDs
    internal static ImmutableDictionary<string, Func<string>> KeyboardSimulatorTriggerGuiTagToLabel = HardwareInputTrigger.GetTriggerGuiIdPairs.ToImmutableDictionary(); // IDs as pairs

    

    #nullable enable
    public static event Action<string>? OnSteamSceneAppIdChanged;
    public static string SteamSceneAppId
    {
        get;
        set
        {
            if (EqualityComparer<string>.Default.Equals(field, value)) return;
            field = value;
            OnSteamSceneAppIdChanged?.Invoke(value);
        }
    } = "";

    public static string SteamSceneAppName { get; set; } = "";
    public static ActionSet[] VrInputActionSets { get; set; } = [];
}

public class ActionGuiEntry(string name, string path, bool isChord, Func<string> prompt)
{
    public readonly string Name = name;
    public readonly string Path = path;
    public readonly bool IsChord = isChord;
    public readonly Func<string> Prompt = prompt;
    public string Id => $"{Prompt()}##{Name}";
}