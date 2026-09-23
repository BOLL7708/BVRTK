using System.Collections.Immutable;
using BVRTKCG.Attributes;

namespace BVRTK.Data.Setting;

[Setting]
public partial class Application
{
    [GuiCheckbox("Launch with SteamVR", "Will register the application to automatically launch with SteamVR.")]
    public bool LaunchWithSteamVr { get; set; } = true;

    [GuiCheckbox("Enable interface gradient", "Will render a gradient that shades the entire app, can be disabled for a flatter look, originally a test setting.")]
    public bool EnableInterfaceGradient { get; set; } = true;

    [GuiCheckbox("Show desktop window on launch", "Will show a mirror of the overlay on the desktop when the application launches.")]
    public bool ShowDesktopWindowOnLaunch { get; set; } = true;

    [GuiCombo("Language", "Set the language of the application.", 256f, nameof(Session) + "." + nameof(Session.SupportedLanguageGuiIds))]
    public string Language { get; set; } = Constants.SystemDefaultLanguage;

    #region Quick settings in sidebar

    public bool ShowTooltips { get; set; } = true;

    #endregion

    #region Invisible dynamically updated values

    public int CurrentSection { get; set; } = 0;

    public ImmutableDictionary<int, int> CurrentPageInSection { get; set; } = [];

    #endregion

    // [GuiDebug("Settings.Current.Application.ShowTooltips")] 
    // private object Debug { get; set; }

    // [GuiTest(true, 1, 1.2f, "Test", [true, false], [0,2], [1.2f, 2.3f], ["Testing", "Arrays"])]
    // private object Test { get; set; }

    // [GuiDebug("string.Join(Environment.NewLine, Settings.Current.Application.CurrentPageInSection)")]
    // private object DebugDictionaryValue { get; set; }
}