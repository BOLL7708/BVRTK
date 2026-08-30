using System.Diagnostics;
using BVRTK.Resources;
using SharpHook.Data;

namespace BVRTK.Components.KeyboardSimulator;

public static class KeyboardSimulatorUtils
{
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

    public static string[] GetGuiTags()
    {
        var keycodes = Enum.GetValues<KeyCode>();
        var functionKeys = keycodes.Where(x => IsFunc(Enum.GetName(x)));
        var singleKeys = keycodes.Where(x => IsSingle(Enum.GetName(x)));
        var rest = keycodes.Where(x => !IsSingle(Enum.GetName(x)) && !IsFunc(Enum.GetName(x)));
        keycodes = [.. functionKeys, .. singleKeys, .. rest];
        
        // Check if we have a value, otherwise take the name and remove Vc prefix.
        var values = new List<string>();
        foreach (var keycode in keycodes)
        {
            if ((int)keycode == 0 || KeyCodeIgnored.Contains(keycode)) continue;
            KeyCodeDisplayValues.TryGetValue(keycode, out var displayOverride);

            // According to [official docs](https://sharphook.tolik.io/articles/keycodes.html) the enum VALUE is flexible between versions and the NAME is the only static reference and what should be used.
            var reference = Enum.GetName(keycode);

            var displayName = reference?[2..] ?? reference;
            values.Add(
                displayOverride.IsWhiteSpace()
                    ? $"{displayName}##{reference}"
                    : $"{displayOverride}##{reference}"
            );
        }

        return [.. values];
        
        static bool IsFunc(string? n) => n?.Length > 3 && n[2] == 'F' && n[3..].All(char.IsDigit); // Function keys
        static bool IsSingle(string? n) => n?.Length == 3; // Letters & digits
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

    public static string GetPromptNameForHardwareInput(HardwareInput hwi)
    {
        var promptName = hwi switch
        {
            HardwareInput.None => nameof(HardwareInputPrompts.None),
            HardwareInput.StickButton => nameof(HardwareInputPrompts.StickButton),
            HardwareInput.StickNorth => nameof(HardwareInputPrompts.StickNorth),
            HardwareInput.StickWest => nameof(HardwareInputPrompts.StickWest),
            HardwareInput.StickSouth => nameof(HardwareInputPrompts.StickSouth),
            HardwareInput.StickEast => nameof(HardwareInputPrompts.StickEast),
            HardwareInput.TrackpadButton => nameof(HardwareInputPrompts.TrackpadButton),
            HardwareInput.TrackpadNorth => nameof(HardwareInputPrompts.TrackpadNorth),
            HardwareInput.TrackpadWest => nameof(HardwareInputPrompts.TrackpadWest),
            HardwareInput.TrackpadSouth => nameof(HardwareInputPrompts.TrackpadSouth),
            HardwareInput.TrackpadEast => nameof(HardwareInputPrompts.TrackpadEast),
            HardwareInput.FaceButtonNorth => nameof(HardwareInputPrompts.FaceButtonNorth),
            HardwareInput.FaceButtonWest => nameof(HardwareInputPrompts.FaceButtonWest),
            HardwareInput.FaceButtonSouth => nameof(HardwareInputPrompts.FaceButtonSouth),
            HardwareInput.FaceButtonEast => nameof(HardwareInputPrompts.FaceButtonEast),
            HardwareInput.SystemButtonNorth => nameof(HardwareInputPrompts.SystemButtonNorth),
            HardwareInput.SystemButtonSouth => nameof(HardwareInputPrompts.SystemButtonSouth),
            HardwareInput.TriggerPrimary => nameof(HardwareInputPrompts.TriggerPrimary),
            HardwareInput.TriggerSecondary => nameof(HardwareInputPrompts.TriggerSecondary),
            HardwareInput.GripTrigger => nameof(HardwareInputPrompts.GripTrigger),
            HardwareInput.GripButton => nameof(HardwareInputPrompts.GripButton),
            HardwareInput.OtherButton1 => nameof(HardwareInputPrompts.OtherButton1),
            HardwareInput.OtherButton2 => nameof(HardwareInputPrompts.OtherButton2),
            HardwareInput.OtherButton3 => nameof(HardwareInputPrompts.OtherButton3),
            HardwareInput.OtherButton4 => nameof(HardwareInputPrompts.OtherButton4),
            HardwareInput.OtherButton5 => nameof(HardwareInputPrompts.OtherButton5),
            HardwareInput.OtherButton6 => nameof(HardwareInputPrompts.OtherButton6),
            HardwareInput.OtherButton7 => nameof(HardwareInputPrompts.OtherButton7),
            HardwareInput.OtherButton8 => nameof(HardwareInputPrompts.OtherButton8),
            HardwareInput.OtherButton9 => nameof(HardwareInputPrompts.OtherButton9),
            HardwareInput.OtherButton10 => nameof(HardwareInputPrompts.OtherButton10),
            HardwareInput.OtherButton11 => nameof(HardwareInputPrompts.OtherButton11),
            HardwareInput.OtherButton12 => nameof(HardwareInputPrompts.OtherButton12),
            HardwareInput.OtherButton13 => nameof(HardwareInputPrompts.OtherButton13),
            HardwareInput.OtherButton14 => nameof(HardwareInputPrompts.OtherButton14),
            HardwareInput.OtherButton15 => nameof(HardwareInputPrompts.OtherButton15),
            HardwareInput.OtherButton16 => nameof(HardwareInputPrompts.OtherButton16),
            HardwareInput.Chord1 => nameof(HardwareInputPrompts.Chord1),
            HardwareInput.Chord2 => nameof(HardwareInputPrompts.Chord2),
            HardwareInput.Chord3 => nameof(HardwareInputPrompts.Chord3),
            HardwareInput.Chord4 => nameof(HardwareInputPrompts.Chord4),
            HardwareInput.Chord5 => nameof(HardwareInputPrompts.Chord5),
            HardwareInput.Chord6 => nameof(HardwareInputPrompts.Chord6),
            HardwareInput.Chord7 => nameof(HardwareInputPrompts.Chord7),
            HardwareInput.Chord8 => nameof(HardwareInputPrompts.Chord8),
            HardwareInput.Chord9 => nameof(HardwareInputPrompts.Chord9),
            HardwareInput.Chord10 => nameof(HardwareInputPrompts.Chord10),
            HardwareInput.Chord11 => nameof(HardwareInputPrompts.Chord11),
            HardwareInput.Chord12 => nameof(HardwareInputPrompts.Chord12),
            HardwareInput.Chord13 => nameof(HardwareInputPrompts.Chord13),
            HardwareInput.Chord14 => nameof(HardwareInputPrompts.Chord14),
            HardwareInput.Chord15 => nameof(HardwareInputPrompts.Chord15),
            HardwareInput.Chord16 => nameof(HardwareInputPrompts.Chord16),
            _ => throw new ArgumentOutOfRangeException(nameof(hwi), hwi, null)
        };
        return promptName;
    }
}