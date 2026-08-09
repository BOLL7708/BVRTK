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
    
    public static readonly List<Section> Sections =
    [
        new("BVRTK", GuiColor.GrayLightest, [
            new Page("Splash", Root.RenderSplashPage),
            new Page("About", Root.RenderAboutPage),
            new Page("Links", Root.RenderLinksPage)
        ]),
        new("Settings", GuiColor.Yellow, [
            new Page("Instructions", GuiRenderers.RenderApplicationPage), 
            new Page("Bingo", PlaceholderRenderer)
        ]),
        new("Server", GuiColor.Green, [
            new Page("Bananas", PlaceholderRenderer), 
            new Page("Error", PlaceholderRenderer), 
            new Page("Help!", PlaceholderRenderer)
        ]),
        new("Keyboard Sim", GuiColor.Cyan, [new Page("Cakes", PlaceholderRenderer)]),
        new("Mouse Sim", GuiColor.Blue, [new Page("Whoops!", PlaceholderRenderer)])
    ];

}

public class Section(string title, Vector4 accentColor, List<Page> pages)
{
    public readonly string Title = title;
    public readonly Vector4 AccentColor = accentColor;
    public readonly List<Page> Pages = pages;
}

public class Page(string title, Action renderer)
{
    public readonly string Title = title;
    public readonly Action Renderer = renderer;
}