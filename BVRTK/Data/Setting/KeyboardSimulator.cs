using System.Collections.Immutable;
using BVRTKCG.Attributes;

namespace BVRTK.Data.Setting;

[Setting]
public partial class KeyboardSimulator
{
    [GuiCheckbox("Enabled", "Enable keyboard simulation")]
    public bool Enabled { get; set; } = true;

    [GuiCheckbox("Haptic pulse on key", "Will trigger a haptic pulse when a key is being simulated.")]
    public bool HapticPulseOnKey { get; set; } = true;

    // [GuiCheckbox("Notification on key", "Will spawn a notification in the headset when a key is being simulated.")]
    public bool NotificationOnKey { get; set; } = false;

    public ImmutableSortedDictionary<string, string[]> EntriesManual { get; set; } = ImmutableSortedDictionary<string, string[]>.Empty;

    public ImmutableDictionary<string, string[]> EntriesPerGame { get; set; } = [];
}