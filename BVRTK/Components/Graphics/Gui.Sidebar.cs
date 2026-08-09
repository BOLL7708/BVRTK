using System.Numerics;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static partial class Gui
{
    private static int _selectedSection = 0;

    private static void RenderSidebar()
    {
        // The sidebar
        ImGui.BeginChild("##Sidebar", new Vector2(Constants.GuiSidebarWidth, 0));

        // To add top space.
        ImGui.Dummy(Vector2.Zero);

        // Section buttons, each colored by its accent

        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, Constants.GuiGlobalRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, ImGui.GetStyle().FramePadding with { X = Constants.GuiGlobalRounding * 2f });
        ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(1f, 0.5f)); // Horizontal and vertical alignment

        var availableSpace = ImGui.GetContentRegionAvail();
        var i = 0;
        foreach (var section in GuiStructure.Sections)
        {
            switch (section.Font)
            {
                case FontStyle.Regular:
                    ImGui.PushFont(Session.GuiFonts.Regular, 0f);
                    break;
                case FontStyle.Bold:
                    ImGui.PushFont(Session.GuiFonts.Bold, 0f);
                    break;
                case FontStyle.Italic:
                    ImGui.PushFont(Session.GuiFonts.Italic, 0f);
                    break;
                case FontStyle.BoldItalic:
                    ImGui.PushFont(Session.GuiFonts.BoldItalic, 0f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var isActive = i == _selectedSection;
            ImGui.PushStyleColor(ImGuiCol.Button, isActive
                ? section.AccentColor.TabActive()
                : section.AccentColor.Tab()
            );
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, isActive
                ? section.AccentColor.TabActive()
                : section.AccentColor.TabHover()
            );
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, section.AccentColor.TabActive());
            ImGui.PushStyleColor(ImGuiCol.Text, isActive
                ? Vector4.Zero with { W = 1f }
                : Vector4.One
            );

            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Constants.GuiGlobalRounding);
            ImGui.Button(section.Title, availableSpace with { Y = 0 });
            if (ImGui.IsItemActivated())
            {
                _selectedSection = i;
            }

            ImGui.PopStyleColor(4);
            ImGui.PopFont();
            i++;
        }

        ImGui.PopStyleVar(3);
        ImGui.EndChild();
    }
}