using BVRTKCG.Attributes;

namespace BVRTK.Data.Setting;

[Setting]
public partial class Screenshots
{
    #region Main
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