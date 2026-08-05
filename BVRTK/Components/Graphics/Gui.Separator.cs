using System.Numerics;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static partial class Gui
{
    private static void RenderSeparator()
    {
        var section = GuiStructure.Sections[_selectedSection];

        // Separator
        ImGui.PushStyleColor(ImGuiCol.ChildBg, section.AccentColor);
        ImGui.BeginChild("##Separator", Vector2.Zero with { X = Constants.GuiSeparatorGirth });
        ImGui.EndChild();
        ImGui.PopStyleColor();
    }
}