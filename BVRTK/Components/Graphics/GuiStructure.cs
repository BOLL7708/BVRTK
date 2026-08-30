using System.Numerics;
using BVRTK.Components.Graphics.Pages;
using BVRTK.Data.Setting;
using BVRTK.Resources;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static class GuiStructure
{
    private static readonly Page PageWip = new Page("WIP", () => { ImGui.TextWrapped("Work in progress, this feature is not yet available."); });

    public static readonly List<Section> Sections =
    [
        new(()=>"Development", ()=>"Only for me!", false, FontStyle.Bold, GuiColor.FromHue(0), [
            new Page("Component Zoo", Develop.RenderZooPage)
        ]),
        new(()=>GuiSidebarPrompts.AppLabel, ()=>GuiSidebarPrompts.AppTooltip, true, FontStyle.Bold, GuiColor.Root, [
            new Page("About", Root.RenderAboutPage),
            new Page("Version History", Root.RenderVersionHistoryPage),
            new Page("Licenses", Root.RenderLicensesPage)
        ]),
        new(()=>GuiSidebarPrompts.PreferencesLabel, ()=>GuiSidebarPrompts.PreferencesTooltip, true, FontStyle.Regular, GuiColor.Preferences, [
            new Page("Options", GuiRenderers.RenderApplicationPage)
        ]),
        new(()=>GuiSidebarPrompts.ServerLabel, ()=>GuiSidebarPrompts.ServerTooltip, true, FontStyle.Regular, GuiColor.Server, [
            new Page("Options", GuiRenderers.RenderServerPage)
        ]),
        new(()=>GuiSidebarPrompts.KeyboardSimLabel, ()=>GuiSidebarPrompts.KeyboardSimTooltip, true, FontStyle.Regular, GuiColor.KeyboardSim, [
            new Page("Options", GuiRenderers.RenderKeyboardSimulatorPage)        
        ]),
        new(()=>GuiSidebarPrompts.MouseSimLabel, ()=>GuiSidebarPrompts.MouseSimTooltip, false, FontStyle.Regular, GuiColor.MouseSim, [PageWip]),
        new(()=>GuiSidebarPrompts.OverlaysLabel, ()=>GuiSidebarPrompts.OverlaysTooltip, false, FontStyle.Regular, GuiColor.Overlays, [PageWip]),
        new(()=>GuiSidebarPrompts.ScreenshotsLabel, ()=>GuiSidebarPrompts.ScreenshotsTooltip, false, FontStyle.Regular, GuiColor.Screenshots, [
            new Page("Options", GuiRenderers.RenderScreenshotsPage)
        ]),
        new(()=>GuiSidebarPrompts.PlayAreaLabel, ()=>GuiSidebarPrompts.PlayAreaTooltip, false, FontStyle.Regular, GuiColor.PlayArea, [PageWip]),
        new(()=>GuiSidebarPrompts.EventsLabel, ()=>GuiSidebarPrompts.EventsTooltip, false, FontStyle.Regular, GuiColor.Events, [PageWip]),
    ];
}

public enum FontStyle
{
    Regular,
    Bold,
    Italic,
    BoldItalic
}

public class Section(Func<string> title, Func<string> tooltip, bool isPublic, FontStyle font, Vector4 accentColor, List<Page> pages)
{
    public readonly Func<string> Title = title;
    public readonly Func<string> Tooltip = tooltip;
    public readonly bool IsPublic = isPublic;
    public readonly FontStyle Font = font;
    public readonly Vector4 AccentColor = accentColor;
    public readonly List<Page> Pages = pages;
}

public class Page(string title, Action renderer)
{
    public readonly string Title = title;
    public readonly Action Renderer = renderer;
}