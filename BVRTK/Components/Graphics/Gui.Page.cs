using System.Numerics;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static partial class Gui
{
    private static readonly Dictionary<int, int> SelectedTabs = new();

    private static void RenderPage()
    {
        var section = GuiStructure.Sections[_selectedSection];
        if (section.Pages.Count == 0) return;

        var i = 0;
        ImGui.PushStyleColor(ImGuiCol.ChildBg, section.AccentColor * 0.30f);
        ImGui.BeginChild($"##Page{section.Title}", Vector2.Zero);
        ImGui.Dummy(Vector2.Zero); // Simply adds the default spacing at the top.
        ImGui.GetStyle().TabBarBorderSize = 0;
        
        ImGui.PushStyleColor(ImGuiCol.TabSelected, section.AccentColor * 0.80f);
        ImGui.PushStyleColor(ImGuiCol.Tab, section.AccentColor * 0.60f);
        ImGui.PushStyleColor(ImGuiCol.TabDimmed, section.AccentColor * 0.40f);
        if (!ImGui.BeginTabBar($"##Tabs{section.Title}"))
        {
            ImGui.EndChild();
            return;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, Constants.GuiGlobalRounding);
        foreach (var page in section.Pages)
        {
            SelectedTabs.TryGetValue(_selectedSection, out var selectedTab);
            var isCurrent = selectedTab == i;
            ImGui.PushStyleColor(ImGuiCol.TabHovered, isCurrent 
                ? section.AccentColor * 0.80f
                : section.AccentColor * 0.70f
            );
            if (ImGui.BeginTabItem(page.Title))
            {
                // Instead of messing with repacing item spacing, we simply move the cursor up the same distance to negate it.
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGui.GetStyle().ItemSpacing.Y);
                
                // Drawing a custom separation line for the tab bar that matches the sidebar.
                ImGui.PushStyleColor(ImGuiCol.ChildBg, section.AccentColor * 0.80f);
                ImGui.BeginChild($"##Separator{section.Title}", Vector2.Zero with { Y = Constants.GuiSeparatorGirth });
                ImGui.EndChild();
                ImGui.PopStyleColor();
                
                SelectedTabs[_selectedSection] = i;

                // Also skip the automatic spacing below the custom line.
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGui.GetStyle().ItemSpacing.Y);
                // The below rendering is the same for all pages right now, but the problem is it ends up at the bottom.
                page.Render(section.AccentColor);

                ImGui.EndTabItem();
            }
            ImGui.PopStyleColor();

            i++;
        }

        ImGui.PopStyleVar();
        ImGui.EndTabBar();
        ImGui.PopStyleColor(3);
        ImGui.EndChild();
        ImGui.PopStyleColor();
    }
}