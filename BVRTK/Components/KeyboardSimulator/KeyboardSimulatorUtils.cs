using System.Diagnostics;
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

    private static readonly List<KeyCode> KeyCodeIgnored = [
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
        foreach(var pair in pairs)
        {
            ids.Add($"{pair.Value}##{pair.Key}");
        }

        return [.. ids];
    }
    
    public static KeyCode TagToEnum(string tag)
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
    
    #endregion

    #region VR Input
    
    public static string GetPromptNameForHardwareInputLeftRight(HardwareInputLeftRight hwi)
    {
        var promptName = hwi switch
        {
            HardwareInputLeftRight.StickNorth => nameof(HardwareInputPrompts.StickNorth),
            HardwareInputLeftRight.StickEast => nameof(HardwareInputPrompts.StickEast),
            HardwareInputLeftRight.StickSouth => nameof(HardwareInputPrompts.StickSouth),
            HardwareInputLeftRight.StickWest => nameof(HardwareInputPrompts.StickWest),
            HardwareInputLeftRight.StickButton => nameof(HardwareInputPrompts.StickButton),
            HardwareInputLeftRight.TrackpadNorth => nameof(HardwareInputPrompts.TrackpadNorth),
            HardwareInputLeftRight.TrackpadEast => nameof(HardwareInputPrompts.TrackpadEast),
            HardwareInputLeftRight.TrackpadSouth => nameof(HardwareInputPrompts.TrackpadSouth),
            HardwareInputLeftRight.TrackpadWest => nameof(HardwareInputPrompts.TrackpadWest),
            HardwareInputLeftRight.TrackpadCenter => nameof(HardwareInputPrompts.TrackpadCenter),
            HardwareInputLeftRight.FaceButtonNorth => nameof(HardwareInputPrompts.FaceButtonNorth),
            HardwareInputLeftRight.FaceButtonEast => nameof(HardwareInputPrompts.FaceButtonEast),
            HardwareInputLeftRight.FaceButtonSouth => nameof(HardwareInputPrompts.FaceButtonSouth),
            HardwareInputLeftRight.FaceButtonWest => nameof(HardwareInputPrompts.FaceButtonWest),
            HardwareInputLeftRight.SystemButtonNorth => nameof(HardwareInputPrompts.SystemButtonNorth),
            HardwareInputLeftRight.SystemButtonSouth => nameof(HardwareInputPrompts.SystemButtonSouth),
            HardwareInputLeftRight.TriggerPrimary => nameof(HardwareInputPrompts.TriggerPrimary),
            HardwareInputLeftRight.TriggerSecondary => nameof(HardwareInputPrompts.TriggerSecondary),
            HardwareInputLeftRight.GripTrigger => nameof(HardwareInputPrompts.GripTrigger),
            HardwareInputLeftRight.GripButton => nameof(HardwareInputPrompts.GripButton),
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