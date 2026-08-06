namespace BVRTK.Data.Setting;

[Setting]
public partial class Application
{
    private bool _launchWithSteamVr = true;
    public partial bool LaunchWithSteamVr { get; set; }

    private bool _enableInterfaceGradient = true;
    public partial bool EnableInterfaceGradient { get; set; }
}