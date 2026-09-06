using System.Collections.Concurrent;
using BVRTKCG.Attributes;

namespace BVRTK.Data.Setting;

[Setting]
public partial class KeyboardSimulator
{
    [GuiCheckbox("Enabled", "Enable keyboard simulation")]
    private bool _enabled = false;
    public partial bool Enabled { get; set; }

    [GuiCheckbox("Haptic pulse on key", "Will trigger a haptic pulse when a key is being simulated.")]
    private bool _hapticPulseOnKey = false;
    public partial bool HapticPulseOnKey { get; set; }

    [GuiCheckbox("Notification on key", "Will spawn a notification in the headset when a key is being simulated.")]
    private bool _notificationOnKey = false;
    public partial bool NotificationOnKey { get; set; }
    
    private string[] _entriesUniversal = [];
    public partial string[] EntriesUniversal { get; set; }
    
    private ConcurrentDictionary<string, string[]> _entriesPerGame = new();
    public partial ConcurrentDictionary<string, string[]> EntriesPerGame { get; set; }
}