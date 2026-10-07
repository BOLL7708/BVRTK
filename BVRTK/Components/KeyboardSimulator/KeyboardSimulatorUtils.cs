using System.Diagnostics;
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

    private static KeyCode NameToEnum(string tag)
    {
        try
        {
            return Enum.Parse<KeyCode>(tag);
        }
        catch (Exception e)
        {
            // TODO: Log this probably
            return KeyCode.VcUndefined;
        }
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
        // TODO: New settings values here later

        // Parse strings and derive indices
        Session.ActionGuiIds.TryGetValue(Constants.ActionSet.KeyboardSim, out var actionEntryLabels);
        var actionIndex = Math.Max(0, (actionEntryLabels ?? []).ToList().FindIndex(it => it.EndsWith($"##{actionStr}")));
        
        Session.ActionEntries.TryGetValue(Constants.ActionSet.KeyboardSim, out var actionEntries); 
        var actionEntry = (actionEntries ?? [])[actionIndex];
        
        var keyCodeIndex = Math.Max(0, Session.KeyboardSimulatorKeyCodeGuiIds.ToList().FindIndex(it => it.EndsWith($"##{keyCodeStr}")));
        var keyCode = NameToEnum(keyCodeStr);
        var modifierFlags = (ModifierFlags)ParseByteFromHexStr(modifierStr);
        // TODO: Use same parser for other checkboxes

        return new SimEntry(actionEntry, actionIndex, keyCode, keyCodeIndex, modifierFlags, label);

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
        var action = GuiUtils.GetTagFromId((actionGuiIds?? [])[entry.ActionIndex]);
        var key = GuiUtils.GetTagFromId(Session.KeyboardSimulatorKeyCodeGuiIds[entry.KeyCodeIndex]);
        return $"{action}|{key}|{(byte)entry.Modifiers:X2} {entry.Label}".Trim();
    }


    internal record struct SimEntry(
        ActionGuiEntry Action,
        int ActionIndex,
        KeyCode KeyCode,
        int KeyCodeIndex,
        ModifierFlags Modifiers,
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
            HardwareInputShared.OtherButton1 => nameof(HardwareInputPrompts.OtherButton1),
            HardwareInputShared.OtherButton2 => nameof(HardwareInputPrompts.OtherButton2),
            HardwareInputShared.OtherButton3 => nameof(HardwareInputPrompts.OtherButton3),
            HardwareInputShared.OtherButton4 => nameof(HardwareInputPrompts.OtherButton4),
            HardwareInputShared.OtherButton5 => nameof(HardwareInputPrompts.OtherButton5),
            HardwareInputShared.OtherButton6 => nameof(HardwareInputPrompts.OtherButton6),
            HardwareInputShared.OtherButton7 => nameof(HardwareInputPrompts.OtherButton7),
            HardwareInputShared.OtherButton8 => nameof(HardwareInputPrompts.OtherButton8),
            HardwareInputShared.OtherButton9 => nameof(HardwareInputPrompts.OtherButton9),
            HardwareInputShared.OtherButton10 => nameof(HardwareInputPrompts.OtherButton10),
            HardwareInputShared.OtherButton11 => nameof(HardwareInputPrompts.OtherButton11),
            HardwareInputShared.OtherButton12 => nameof(HardwareInputPrompts.OtherButton12),
            HardwareInputShared.OtherButton13 => nameof(HardwareInputPrompts.OtherButton13),
            HardwareInputShared.OtherButton14 => nameof(HardwareInputPrompts.OtherButton14),
            HardwareInputShared.OtherButton15 => nameof(HardwareInputPrompts.OtherButton15),
            HardwareInputShared.OtherButton16 => nameof(HardwareInputPrompts.OtherButton16),
            HardwareInputShared.Chord1 => nameof(HardwareInputPrompts.Chord1),
            HardwareInputShared.Chord2 => nameof(HardwareInputPrompts.Chord2),
            HardwareInputShared.Chord3 => nameof(HardwareInputPrompts.Chord3),
            HardwareInputShared.Chord4 => nameof(HardwareInputPrompts.Chord4),
            HardwareInputShared.Chord5 => nameof(HardwareInputPrompts.Chord5),
            HardwareInputShared.Chord6 => nameof(HardwareInputPrompts.Chord6),
            HardwareInputShared.Chord7 => nameof(HardwareInputPrompts.Chord7),
            HardwareInputShared.Chord8 => nameof(HardwareInputPrompts.Chord8),
            HardwareInputShared.Chord9 => nameof(HardwareInputPrompts.Chord9),
            HardwareInputShared.Chord10 => nameof(HardwareInputPrompts.Chord10),
            HardwareInputShared.Chord11 => nameof(HardwareInputPrompts.Chord11),
            HardwareInputShared.Chord12 => nameof(HardwareInputPrompts.Chord12),
            HardwareInputShared.Chord13 => nameof(HardwareInputPrompts.Chord13),
            HardwareInputShared.Chord14 => nameof(HardwareInputPrompts.Chord14),
            HardwareInputShared.Chord15 => nameof(HardwareInputPrompts.Chord15),
            HardwareInputShared.Chord16 => nameof(HardwareInputPrompts.Chord16),
            _ => throw new ArgumentOutOfRangeException(nameof(hwi), hwi, null)
        };
        return promptName;
    }

    #endregion
}