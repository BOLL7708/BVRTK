using System.Numerics;
using BVRTK.Components.Graphics.Pages;
using BVRTK.Data;
using BVRTK.Data.Setting;
using BVRTK.Resources;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static class GuiStructure
{
    private static readonly Page PageWip = new Page("WIP", () => { ImGui.TextWrapped("Work in progress, this feature is not yet available."); });

    public static readonly List<Section> Sections =
    [
        new(() => "Development", () => "Only for me!", isPublic: false, font: FontStyle.Bold, accentColor: GuiColor.FromHue(0), pages: [
            new Page("Component Zoo", Develop.RenderZooPage)
        ]),
        new(() => GuiSidebarPrompts.AppLabel, () => GuiSidebarPrompts.AppTooltip, font: FontStyle.Bold, accentColor: GuiColor.Root, pages: [
            new Page("About", Root.RenderAboutPage),
            new Page("Version History", Root.RenderVersionHistoryPage),
            new Page("Licenses", Root.RenderLicensesPage)
        ]),
        new(() => GuiSidebarPrompts.PreferencesLabel, () => GuiSidebarPrompts.PreferencesTooltip, accentColor: GuiColor.Preferences, pages: [
            new Page("Options", GuiRenderers.RenderApplicationPage)
        ]),
        new(() => GuiSidebarPrompts.ServerLabel, () => GuiSidebarPrompts.ServerTooltip, ()=>Settings.Current.Server.Enabled, accentColor: GuiColor.Server, pages: [
            new Page("Options", GuiRenderers.RenderServerPage)
        ]),
        new(() => GuiSidebarPrompts.KeyboardSimLabel, () => GuiSidebarPrompts.KeyboardSimTooltip, ()=>Settings.Current.KeyboardSimulator.Enabled, accentColor: GuiColor.KeyboardSim, pages: [
            new Page("Options", GuiRenderers.RenderKeyboardSimulatorPage)
        ]),
        new(() => GuiSidebarPrompts.MouseSimLabel, () => GuiSidebarPrompts.MouseSimTooltip, isPublic: false, accentColor: GuiColor.MouseSim, pages: [PageWip]),
        new(() => GuiSidebarPrompts.OverlaysLabel, () => GuiSidebarPrompts.OverlaysTooltip, isPublic: false, accentColor: GuiColor.Overlays, pages: [PageWip]),
        new(() => GuiSidebarPrompts.ScreenshotsLabel, () => GuiSidebarPrompts.ScreenshotsTooltip, isPublic: false, accentColor: GuiColor.Screenshots, pages: [
            new Page("Options", GuiRenderers.RenderScreenshotsPage)
        ]),
        new(() => GuiSidebarPrompts.PlayAreaLabel, () => GuiSidebarPrompts.PlayAreaTooltip, isPublic: false, accentColor: GuiColor.PlayArea, pages: [PageWip]),
        new(() => GuiSidebarPrompts.EventsLabel, () => GuiSidebarPrompts.EventsTooltip, isPublic: false, accentColor: GuiColor.Events, pages: [PageWip]),
    ];
}

public enum FontStyle
{
    Regular,
    Bold,
    Italic,
    BoldItalic
}

public class Section(
    Func<string> title,
    Func<string>? tooltip = null,
    Func<bool>? isEnabled = null,
    bool isPublic = true,
    FontStyle font = FontStyle.Regular,
    Vector4? accentColor = null,
    List<Page>? pages = null
)
{
    public readonly Func<string> Title = title;
    public readonly Func<string> Tooltip = tooltip ?? (() => "");
    public readonly Func<bool> IsEnabled = isEnabled ?? (() => true);
    public readonly bool IsPublic = isPublic;
    public readonly FontStyle Font = font;
    public readonly Vector4 AccentColor = accentColor ?? GuiColor.Gray;
    public readonly List<Page> Pages = pages ?? [];
}

public class Page(string title, Action renderer)
{
    public readonly string Title = title;
    public readonly Action Renderer = renderer;
}