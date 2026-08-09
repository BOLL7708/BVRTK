using System.Numerics;
using BVRTK.Components.Graphics.Pages;
using BVRTK.Data.Setting;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static class GuiStructure
{
    // TODO: Temporary while building this out.
    private static readonly Action PlaceholderRenderer = () =>
    {
        for(var i=0; i<100; i++) ImGui.Text($"PlaceHolder Renderer: Row #{i}");
    };

    private static readonly Page PageWip = new Page("WIP", () => { ImGui.TextWrapped("Work in progress, this feature is not yet available."); });

    private static int _currentColor = 0;
    private const int NumOfColors = 8;

    private static Vector4 GetNextColor()
    {
        const float startValue = 0.15f;
        const float endValue = 0.60f;
        var value = 1f / NumOfColors * _currentColor;
        var hue = (endValue - startValue) * value + startValue;
        _currentColor++;
        return GuiColor.FromHue(hue);
    }
    
    public static readonly List<Section> Sections =
    [
        new("Development", FontStyle.Bold, GuiColor.FromHue(0), [
            new Page("Component Zoo", Develop.RenderZooPage)
        ]),
        new("BVRTK", FontStyle.Bold, GuiColor.GrayLighter, [
            new Page("About", Root.RenderAboutPage),
            new Page("Links", Root.RenderLinksPage),
            new Page ("Version History", Root.RenderVersionHistoryPage)
        ]),
        new("Preferences", FontStyle.Regular, GetNextColor(), [
            new Page("Options", GuiRenderers.RenderApplicationPage), 
            new Page("Bingo", PlaceholderRenderer)
        ]),
        new("Server", FontStyle.Regular, GetNextColor(), [
            new Page("Options", PlaceholderRenderer), 
            new Page("Error", PlaceholderRenderer), 
            new Page("Help!", PlaceholderRenderer)
        ]),
        new("Keyboard Sim", FontStyle.Regular, GetNextColor(), [PageWip]),
        new("Mouse Sim", FontStyle.Regular, GetNextColor(), [PageWip]),
        new("Overlays", FontStyle.Regular, GetNextColor(), [PageWip]),
        new("Screenshots", FontStyle.Regular, GetNextColor(), [PageWip]),
        new("Play Area", FontStyle.Regular, GetNextColor(), [PageWip]),
        new("Events", FontStyle.Regular, GetNextColor(), [PageWip]),
    ];
}

public enum FontStyle
{
    Regular,
    Bold,
    Italic,
    BoldItalic
}

public class Section(string title, FontStyle font, Vector4 accentColor, List<Page> pages)
{
    public readonly string Title = title;
    public readonly FontStyle Font = font;
    public readonly Vector4 AccentColor = accentColor;
    public readonly List<Page> Pages = pages;
}

public class Page(string title, Action renderer)
{
    public readonly string Title = title;
    public readonly Action Renderer = renderer;
}