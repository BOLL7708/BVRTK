using BVRTKCG.Attributes;

namespace BVRTK.Data.Setting;

[Setting]
public partial class Application
{
    [GuiCheckbox("Launch with SteamVR", "Will register the application to automatically launch with SteamVR. You can launching applications under Startup/Shutdown in the SteamVR settings.")]
    private bool _launchWithSteamVr = true;
    public partial bool LaunchWithSteamVr { get; set; }

    [GuiCheckbox("Enable interface gradient", "Will render a gradient that shades the entire app, can be disabled for whatever reason, originally a test setting.")]
    private bool _enableInterfaceGradient = true;
    public partial bool EnableInterfaceGradient { get; set; }

    [GuiCheckbox("Show desktop window on launch", "Will show a mirror of the overlay on the desktop when the application launches.")]
    private bool _showDesktopWindowOnLaunch = true;
    public partial bool ShowDesktopWindowOnLaunch { get; set; }

    [GuiCheckbox("Show tooltips", "VERY BIG TOOLTIPS!")] 
    private bool _showTooltips = true;
    public partial bool ShowTooltips { get; set; }
    
    // [GuiDebug("Settings.Current.Application.ShowTooltips")] 
    // private object Debug { get; set; }
}