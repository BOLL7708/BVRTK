using System.Numerics;
using BVRTK.Components.Graphics.Pages;
using BVRTK.Data.Setting;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static class GuiStructure
{
    private static readonly Page PageWip = new Page("WIP", () => { ImGui.TextWrapped("Work in progress, this feature is not yet available."); });

    public static readonly List<Section> Sections =
    [
        new("Development", false, FontStyle.Bold, GuiColor.FromHue(0), [
            new Page("Component Zoo", Develop.RenderZooPage)
        ]),
        new("BVRTK", true, FontStyle.Bold, GuiColor.Root, [
            new Page("About", Root.RenderAboutPage),
            new Page("Version History", Root.RenderVersionHistoryPage),
            new Page("Licenses", Root.RenderLicensesPage)
        ]),
        new("Preferences", true, FontStyle.Regular, GuiColor.Preferences, [
            new Page("Options", GuiRenderers.RenderApplicationPage),
        ]),
        new("Server", true, FontStyle.Regular, GuiColor.Server, [PageWip]),
        new("Keyboard Sim", false, FontStyle.Regular, GuiColor.KeyboardSim, [PageWip]),
        new("Mouse Sim", false, FontStyle.Regular, GuiColor.MouseSim, [PageWip]),
        new("Overlays", false, FontStyle.Regular, GuiColor.Overlays, [PageWip]),
        new("Screenshots", true, FontStyle.Regular, GuiColor.Screenshots, [PageWip]),
        new("Play Area", false, FontStyle.Regular, GuiColor.PlayArea, [PageWip]),
        new("Events", false, FontStyle.Regular, GuiColor.Events, [PageWip]),
    ];
}

public enum FontStyle
{
    Regular,
    Bold,
    Italic,
    BoldItalic
}

public class Section(string title, bool isPublic, FontStyle font, Vector4 accentColor, List<Page> pages)
{
    public readonly string Title = title;
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