using System.Globalization;
using BVRTK.Components.Graphics;
using BVRTK.Resources;
using SharpHook.Data;

namespace BVRTK.Components.KeyboardSimulator;

public static class KeyboardSimulatorUtils
{
    #region Keys

    private static readonly Dictionary<KeyCode, string> KeyCodeDisplayValues = new()
    {
        [KeyCode.VcAccept] = "CUSTOM LABEL YO!"
    };

    private static readonly List<KeyCode> KeyCodeIgnored =
    [
        KeyCode.VcLeftAlt,
        KeyCode.VcRightAlt,
        KeyCode.VcLeftControl,
        KeyCode.VcRightControl,
        KeyCode.VcLeftShift,
        KeyCode.VcRightShift,
        KeyCode.VcLeftMeta,
        KeyCode.VcRightMeta
    ];

    public static Dictionary<string, string> GetGuiIdPairs()
    {
        var keycodes = Enum.GetValues<KeyCode>();
        var functionKeys = keycodes.Where(x => IsFunc(Enum.GetName(x)));
        var singleKeys = keycodes.Where(x => IsSingle(Enum.GetName(x)));
        var rest = keycodes.Where(x => !IsSingle(Enum.GetName(x)) && !IsFunc(Enum.GetName(x)));
        keycodes = [.. functionKeys, .. singleKeys, .. rest];

        // Check if we have a value, otherwise take the name and remove Vc prefix.
        var values = new Dictionary<string, string>();
        foreach (var keycode in keycodes)
        {
            if ((int)keycode == 0 || KeyCodeIgnored.Contains(keycode)) continue;
            KeyCodeDisplayValues.TryGetValue(keycode, out var displayOverride);

            // According to [official docs](https://sharphook.tolik.io/articles/keycodes.html) the enum VALUE is
            // flexible between versions and the NAME is the only static reference and what should be used.
            var reference = Enum.GetName(keycode) ?? "";
            if (reference.IsWhiteSpace()) continue;

            var displayName = reference[2..];
            values.Add(reference, displayOverride.IsWhiteSpace() ? displayName : displayOverride ?? "N/A");
        }

        return values;

        static bool IsFunc(string? n) => n?.Length > 3 && n[2] == 'F' && n[3..].All(char.IsDigit); // Function keys
        static bool IsSingle(string? n) => n?.Length == 3; // Letters & digits
    }

    public static string[] GetGuiIds()
    {
        var ids = new List<string>();
        var pairs = GetGuiIdPairs();
        foreach (var pair in pairs)
        {
            ids.Add($"{pair.Value}##{pair.Key}");
        }

        return [.. ids];
    }

    internal static SimEntry ParseEntry(string value)
    {
        // Split on the label, which is the first space, then the label can contain anything.
        var firstSpace = value.IndexOf(' ');
        var settings = firstSpace >= 0 ? value[..firstSpace] : value;
        var label = firstSpace >= 0 ? value[(firstSpace + 1)..] : string.Empty;

        // Split on the settings divider
        var parts = settings.Split("|");
        var actionStr = parts.ElementAtOrDefault(0) ?? string.Empty;
        var keyCodeStr = parts.ElementAtOrDefault(1) ?? string.Empty;
        var modifierStr = parts.ElementAtOrDefault(2) ?? "00";
        var triggerStr = parts.ElementAtOrDefault(3) ?? string.Empty;
        var triggerIntervalStr = parts.ElementAtOrDefault(4) ?? "100";

        // Parse strings and derive indices
        Session.ActionGuiIds.TryGetValue(Constants.ActionSet.KeyboardSim, out var actionEntryLabels);
        var actionIndex = Math.Max(0, (actionEntryLabels ?? []).ToList().FindIndex(it => it.EndsWith($"##{actionStr}")));

        Session.ActionEntries.TryGetValue(Constants.ActionSet.KeyboardSim, out var actionEntries);
        var actionEntry = (actionEntries ?? [])[actionIndex];

        var keyCodeIndex = Math.Max(0, Session.KeyboardSimulatorKeyCodeGuiIds.ToList().FindIndex(it => it.EndsWith($"##{keyCodeStr}")));
        Enum.TryParse<KeyCode>(keyCodeStr, true, out var keyCode);
        
        var modifierFlags = (ModifierFlags)ParseByteFromHexStr(modifierStr);

        var triggerIndex = Math.Max(0, Session.KeyboardSimulatorTriggerGuiIds.ToList().FindIndex(it => it.EndsWith($"##{triggerStr}")));
        Enum.TryParse<HardwareInputTrigger>(triggerStr, true, out var trigger);
        int.TryParse(triggerIntervalStr, NumberStyles.None, CultureInfo.InvariantCulture, out var triggerInterval);

        // Output entry
        return new SimEntry(actionEntry, actionIndex, keyCode, keyCodeIndex, modifierFlags, trigger, triggerIndex, triggerInterval, label);

        byte ParseByteFromHexStr(string hexStr)
        {
            if (byte.TryParse(
                    hexStr,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out var result)
               )
            {
                return result;
            }

            return 0;
        }
    }

    internal static string EncodeEntry(SimEntry entry)
    {
        Session.ActionGuiIds.TryGetValue(Constants.ActionSet.KeyboardSim, out var actionGuiIds);
        var action = GuiUtils.GetTagFromId((actionGuiIds ?? [])[entry.ActionIndex]);
        var key = GuiUtils.GetTagFromId(Session.KeyboardSimulatorKeyCodeGuiIds[entry.KeyCodeIndex]);
        var trigger = GuiUtils.GetTagFromId(Session.KeyboardSimulatorTriggerGuiIds[entry.TriggerIndex]);
        return $"{action}|{key}|{(byte)entry.Modifiers:X2}|{trigger}|{entry.TriggerInterval} {entry.Label}".Trim();
    }


    internal record struct SimEntry(
        ActionGuiEntry Action,
        int ActionIndex,
        KeyCode KeyCode,
        int KeyCodeIndex,
        ModifierFlags Modifiers,
        HardwareInputTrigger Trigger,
        int TriggerIndex,
        int TriggerInterval,
        string Label
    );

    [Flags]
    public enum ModifierFlags : byte
    {
        None = 0,

        AltLeft = 1 << 0,
        AltRight = 1 << 1,

        CtrlLeft = 1 << 2,
        CtrlRight = 1 << 3,

        ShiftLeft = 1 << 4,
        ShiftRight = 1 << 5,

        MetaLeft = 1 << 6,
        MetaRight = 1 << 7
    }

    public static KeyCode[] ToKeyCodes(this ModifierFlags modifiers)
    {
        var result = new List<KeyCode>(8);

        if (modifiers.HasFlag(ModifierFlags.AltLeft)) result.Add(KeyCode.VcLeftAlt);
        if (modifiers.HasFlag(ModifierFlags.AltRight)) result.Add(KeyCode.VcRightAlt);
        if (modifiers.HasFlag(ModifierFlags.CtrlLeft)) result.Add(KeyCode.VcLeftControl);
        if (modifiers.HasFlag(ModifierFlags.CtrlRight)) result.Add(KeyCode.VcRightControl);
        if (modifiers.HasFlag(ModifierFlags.ShiftLeft)) result.Add(KeyCode.VcLeftShift);
        if (modifiers.HasFlag(ModifierFlags.ShiftRight)) result.Add(KeyCode.VcRightShift);
        if (modifiers.HasFlag(ModifierFlags.MetaLeft)) result.Add(KeyCode.VcLeftMeta);
        if (modifiers.HasFlag(ModifierFlags.MetaRight)) result.Add(KeyCode.VcRightMeta);

        return [.. result];
    }

    #endregion

    #region VR Input

    public static string GetPromptNameForHardwareInputLeftRight(HardwareInputEnums hwi)
    {
        var promptName = hwi switch
        {
            HardwareInputEnums.StickNorth => nameof(HardwareInputPrompts.StickNorth),
            HardwareInputEnums.StickEast => nameof(HardwareInputPrompts.StickEast),
            HardwareInputEnums.StickSouth => nameof(HardwareInputPrompts.StickSouth),
            HardwareInputEnums.StickWest => nameof(HardwareInputPrompts.StickWest),
            HardwareInputEnums.StickButton => nameof(HardwareInputPrompts.StickButton),
            HardwareInputEnums.TrackpadNorth => nameof(HardwareInputPrompts.TrackpadNorth),
            HardwareInputEnums.TrackpadEast => nameof(HardwareInputPrompts.TrackpadEast),
            HardwareInputEnums.TrackpadSouth => nameof(HardwareInputPrompts.TrackpadSouth),
            HardwareInputEnums.TrackpadWest => nameof(HardwareInputPrompts.TrackpadWest),
            HardwareInputEnums.TrackpadCenter => nameof(HardwareInputPrompts.TrackpadCenter),
            HardwareInputEnums.FaceButtonNorth => nameof(HardwareInputPrompts.FaceButtonNorth),
            HardwareInputEnums.FaceButtonEast => nameof(HardwareInputPrompts.FaceButtonEast),
            HardwareInputEnums.FaceButtonSouth => nameof(HardwareInputPrompts.FaceButtonSouth),
            HardwareInputEnums.FaceButtonWest => nameof(HardwareInputPrompts.FaceButtonWest),
            HardwareInputEnums.SystemButtonNorth => nameof(HardwareInputPrompts.SystemButtonNorth),
            HardwareInputEnums.SystemButtonSouth => nameof(HardwareInputPrompts.SystemButtonSouth),
            HardwareInputEnums.TriggerPrimary => nameof(HardwareInputPrompts.TriggerPrimary),
            HardwareInputEnums.TriggerSecondary => nameof(HardwareInputPrompts.TriggerSecondary),
            HardwareInputEnums.GripTrigger => nameof(HardwareInputPrompts.GripTrigger),
            HardwareInputEnums.GripButton => nameof(HardwareInputPrompts.GripButton),
            _ => throw new ArgumentOutOfRangeException(nameof(hwi), hwi, null)
        };
        return promptName;
    }

    public static string GetPromptNameForHardwareInputShared(HardwareInputShared hwi)
    {
        var promptName = hwi switch
        {
            HardwareInputShared.OtherButton1 or HardwareInputShared.OtherButton2 or HardwareInputShared.OtherButton3
                or HardwareInputShared.OtherButton4 or HardwareInputShared.OtherButton5
                or HardwareInputShared.OtherButton6 or HardwareInputShared.OtherButton7
                or HardwareInputShared.OtherButton8 or HardwareInputShared.OtherButton9
                or HardwareInputShared.OtherButton10 or HardwareInputShared.OtherButton11
                or HardwareInputShared.OtherButton12 or HardwareInputShared.OtherButton13
                or HardwareInputShared.OtherButton14 or HardwareInputShared.OtherButton15
                or HardwareInputShared.OtherButton16 => nameof(HardwareInputPrompts.OtherButton),
            HardwareInputShared.Chord1 or HardwareInputShared.Chord2 or HardwareInputShared.Chord3
                or HardwareInputShared.Chord4 or HardwareInputShared.Chord5 or HardwareInputShared.Chord6
                or HardwareInputShared.Chord7 or HardwareInputShared.Chord8 or HardwareInputShared.Chord9
                or HardwareInputShared.Chord10 or HardwareInputShared.Chord11 or HardwareInputShared.Chord12
                or HardwareInputShared.Chord13 or HardwareInputShared.Chord14 or HardwareInputShared.Chord15
                or HardwareInputShared.Chord16 => nameof(HardwareInputPrompts.Chord),
            _ => ""
        };
        return promptName;
    }

    #endregion
}