using System.Collections.Immutable;
using BVRTKCG.Attributes;

namespace BVRTK.Data.Setting;

[Setting]
public partial class KeyboardSimulator
{
    [GuiCheckbox("Enabled", "Enable keyboard simulation")]
    public bool Enabled { get; set; } = false;

    [GuiCheckbox("Haptic pulse on key", "Will trigger a haptic pulse when a key is being simulated.")]
    public bool HapticPulseOnKey { get; set; } = false;

    [GuiCheckbox("Notification on key", "Will spawn a notification in the headset when a key is being simulated.")]
    public bool NotificationOnKey { get; set; } = false;

    public ImmutableList<string> EntriesGeneral { get; set; } = [];

    public ImmutableDictionary<string, string[]> EntriesPerGame { get; set; } = [];
}