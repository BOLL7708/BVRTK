using System.Numerics;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static class GuiStructure
{
    public static readonly List<Section> Sections =
    [
        new("Settings", Colors.Yellow, [new Page("Instructions"), new Page("Bingo")]),
        new("Server", Colors.Green, [new Page("Bananas"), new Page("Error"), new Page("Help!")]),
        new("Keyboard Sim", Colors.Cyan, [new Page("Cakes")]),
        new("Mouse Sim", Colors.Blue, [new Page("Whoops!")])
    ];

    private static class Colors
    {
        public static readonly Vector4 Yellow = new Vector4(0.9f, 0.8f, 0f, 1f);
        public static readonly Vector4 Green = new Vector4(0f, 1f, 0f, 1f);
        public static readonly Vector4 Cyan = new Vector4(0f, 0.75f, 0.5f, 1f);
        public static readonly Vector4 Blue = new Vector4(0f, 0f, 1f, 1f);
    }
}

public class Section(string title, Vector4 accentColor, List<Page> pages)
{
    public readonly string Title = title;
    public readonly Vector4 AccentColor = accentColor;
    public readonly List<Page> Pages = pages;
}

public class Page(string title)
{
    public readonly string Title = title;

    public void Render(Vector4 accentColor)
    {
        // TODO: Temp

        ImGui.BeginChild("##Title");
        ImGui.Text(Title);
        ImGui.EndChild();
    }
}