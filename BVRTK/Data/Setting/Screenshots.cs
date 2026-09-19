using BVRTKCG.Attributes;

namespace BVRTK.Data.Setting;

[Setting]
public partial class Screenshots
{
    #region Main

    [GuiTitle("Hotkeys", "")]
    [GuiCheckbox("Enable", "Will enable global hotkeys to trigger the features below.")]
    public bool EnableGlobalHotkeys { get; set; } = false;

    [GuiLabel("Take screenshot", true)]
    [GuiCheckbox("Alt##screenshot", "")]
    [GuiSameLine]
    public bool TakeScreenshotAltKey { get; set; } = false;

    [GuiCheckbox("Control##screenshot", "")]
    [GuiSameLine]
    public bool TakeScreenshotControlKey { get; set; } = false;

    [GuiCheckbox("Shift##screenshot", "")]
    [GuiSameLine]
    public bool TakeScreenshotShiftKey { get; set; } = false;

    [GuiCombo(
        "Key##screenshot",
        "Pick a key to be used with the modifiers to trigger this action.",
        180f,
        nameof(Session) + "." + nameof(Session.KeyboardSimulatorKeyCodeGuiTags)
    )]
    public string TakeScreenshotKey { get; set; } = "";

    [GuiLabel("Show viewfinder", true)]
    [GuiCheckbox("Alt##viewfinder", "")]
    [GuiSameLine]
    public bool ShowViewfinderAltKey { get; set; } = false;

    [GuiCheckbox("Control##viewfinder", "")]
    [GuiSameLine]
    public bool ShowViewfinderControlKey { get; set; } = false;

    [GuiCheckbox("Shift##viewfinder", "")]
    [GuiSameLine]
    public bool ShowViewfinderShiftKey { get; set; } = false;

    [GuiCombo(
        "Key##viewfinder",
        "Pick a key to be used with the modifiers to trigger this action.",
        180f,
        nameof(Session) + "." + nameof(Session.KeyboardSimulatorKeyCodeGuiTags)
    )]
    public string ShowViewfinderKey { get; set; } = "";

    [GuiTitle("Notifications & Audio", "")]
    [GuiTitle("Viewfinder", "")]
    [GuiFloatSlider("Float Slider Test", "This is it!", -10f, 10f, "%.2f")]
    public float TestFloatSlider { get; set; } = 0f;

    [GuiIntSlider("Int Slider Test", "This is also it!", -5, 15)]
    public int TestIntSlider { get; set; } = 0;

    #endregion

    #region Time-lapse

    [GuiTitle("Time-lapse", "")]
    [GuiCheckbox("Enable time-lapse capture", "Will automatically and silently capture a screenshot at a specific interval when a scene application is running.")]
    public bool TimerEnabled { get; set; } = false;

    [GuiInt("Time-lapse interval in seconds", "The interval the time-lapse will capture at.", 96f, 1)]
    public int TimerIntervalS { get; set; } = 10;

    #endregion
}