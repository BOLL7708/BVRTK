using BVRTKCG.Attributes;

namespace BVRTK.Data.Setting;

[Setting]
public partial class Screenshots
{
    #region Main
    
    [GuiTitle("Hotkeys", "")]
    [GuiCheckbox("Enable", "Will enable global hotkeys to trigger the features below.")]
    private bool _enableGlobalHotkeys = false;
    public partial bool EnableGlobalHotkeys { get; set; }
    
    [GuiLabel("Take screenshot", true)]
    [GuiCheckbox("Alt##screenshot", "")]
    [GuiSameLine]
    private bool _takeScreenshotAltKey = false;
    public partial bool TakeScreenshotAltKey { get; set; }
    
    [GuiCheckbox("Control##screenshot", "")]
    [GuiSameLine]
    private bool _takeScreenshotControlKey = false;
    public partial bool TakeScreenshotControlKey { get; set; }
    
    [GuiCheckbox("Shift##screenshot", "")]
    private bool _takeScreenshotShiftKey = false;
    public partial bool TakeScreenshotShiftKey { get; set; }
    
    // TODO: [GuiCombo]
    
    [GuiLabel("Show viewfinder", true)]
    [GuiCheckbox("Alt##viewfinder", "")]
    [GuiSameLine]
    private bool _showViewfinderAltKey = false;
    public partial bool ShowViewfinderAltKey { get; set; }
    
    [GuiCheckbox("Control##viewfinder", "")]
    [GuiSameLine]
    private bool _showViewfinderControlKey = false;
    public partial bool ShowViewfinderControlKey { get; set; }
    
    [GuiCheckbox("Shift##viewfinder", "")]
    private bool _showViewfinderShiftKey = false;
    public partial bool ShowViewfinderShiftKey { get; set; }
    
    [GuiTitle("Notifications & Audio", "")] 
    
    
    [GuiTitle("Viewfinder", "")]
    
    [GuiFloatSlider("Float Slider Test", "This is it!", -10f, 10f, "%.2f")]
    private float _testFloatSlider = 0f;
    public partial float TestFloatSlider { get; set; }

    [GuiIntSlider("Int Slider Test", "This is also it!", -5, 15)]
    private int _testIntSlider = 0;
    public partial int TestIntSlider { get; set; }
    
    #endregion
  
    #region Time-lapse

    [GuiTitle("Time-lapse", "")]
    [GuiCheckbox("Enable time-lapse capture", "Will automatically and silently capture a screenshot at a specific interval when a scene application is running.")]
    private bool _timerEnabled = false;

    public partial bool TimerEnabled { get; set; }

    [GuiInt("Time-lapse interval in seconds", "The interval the time-lapse will capture at.", 96f, 1)]
    private int _timerIntervalS = 10;

    public partial int TimerIntervalS { get; set; }

    #endregion
}