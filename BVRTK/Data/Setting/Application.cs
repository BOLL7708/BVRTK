namespace BVRTK.Data.Setting;

[TrackDirty]
public partial class Application
{
    private bool _launchWithSteamVr = true;
    public partial bool LaunchWithSteamVr { get; set; }
    
    private bool _showTrayIcon = true;
    public partial bool ShowTrayIcon { get; set; }
    
    private bool _hideFromTaskbar = true;
    public partial bool HideFromTaskbar { get; set; }
}