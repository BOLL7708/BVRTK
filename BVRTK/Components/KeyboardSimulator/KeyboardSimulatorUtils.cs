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

    public static string[] GetKeyValues()
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
}