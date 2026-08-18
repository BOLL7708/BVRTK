using System.Numerics;
using BVRTK.Data;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static partial class Gui
{
    private static void RenderSeparator()
    {
        var section = GuiStructure.Sections[Settings.Current.Application.CurrentSection];

        // Separator
        ImGui.PushStyleColor(ImGuiCol.ChildBg, section.AccentColor);
        ImGui.BeginChild("##Separator", Vector2.Zero with { X = Constants.GuiMainSeparatorGirth });
        ImGui.EndChild();
        ImGui.PopStyleColor();
    }
}