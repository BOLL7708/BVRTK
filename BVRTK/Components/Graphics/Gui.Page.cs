using System.Numerics;
using BVRTK.Data;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static partial class Gui
{
    private static readonly HashSet<int> _restoredSections = [];
    
    private static void RenderPage()
    {
        var sectionIndex = Settings.Current.Application.CurrentSection;
        var section = GuiStructure.Sections[sectionIndex];
        if (section.Pages.Count == 0) return;

        var i = 0;
        ImGui.BeginChild($"##Page{section.Title}", Vector2.Zero);
        ImGui.Dummy(Vector2.Zero); // Add the default spacing at the top.
        ImGui.GetStyle().TabBarBorderSize = 0;

        ImGui.PushStyleColor(ImGuiCol.TabSelected, section.AccentColor.TabActive());
        ImGui.PushStyleColor(ImGuiCol.Tab, section.AccentColor.Tab());
        ImGui.PushStyleColor(ImGuiCol.TabDimmed, section.AccentColor.Tab()); // Not really used but set just in case

        var startPos = ImGui.GetCursorScreenPos(); // Used for floating text
        
        ImGui.BeginTabBar($"##Tabs{section.Title}");

        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, Constants.GuiTabRounding);
        Settings.Current.Application.CurrentPageInSection.TryGetValue(sectionIndex, out var selectedTab);
        var alreadyRestored = _restoredSections.Contains(sectionIndex);
        
        foreach (var page in section.Pages)
        {
            var isActive = selectedTab == i;
            ImGui.PushStyleColor(ImGuiCol.TabHovered, isActive
                ? section.AccentColor.TabActive()
                : section.AccentColor.TabHover()
            );
            ImGui.PushStyleColor(ImGuiCol.Text, isActive
                ? GuiColor.Black
                : GuiColor.White
            );
            
            var tabFlags = ImGuiTabItemFlags.None;
            if (!alreadyRestored && isActive) tabFlags |= ImGuiTabItemFlags.SetSelected;

            var tabSelected = ImGui.BeginTabItem(page.Title, tabFlags);
            ImGui.PopStyleColor();
            if (tabSelected)
            {
                // Instead of messing with replacing item spacing, we simply move the cursor up the same distance to negate it.
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGui.GetStyle().ItemSpacing.Y);

                // Drawing a custom separation line for the tab bar that matches the sidebar.
                ImGui.PushStyleColor(ImGuiCol.ChildBg, section.AccentColor.TabActive());
                ImGui.BeginChild($"##Separator{section.Title}", Vector2.Zero with { Y = Constants.GuiMainSeparatorGirth });
                ImGui.EndChild();
                ImGui.PopStyleColor();

                if (alreadyRestored)
                {
                    // Store that this tab was selected, helps with color states.
                    Settings.Current.Application.InternalCurrentPageInSectionSet(sectionIndex, i);
                }
                
                // Also skip the automatic spacing below the custom line.
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGui.GetStyle().ItemSpacing.Y);

                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Constants.GuiItemSpacing);
                GuiUtils.PushColorAccents(section.AccentColor);
                
                // Turns out window padding is ignored by default without a border.
                ImGui.BeginChild($"##Content{section.Title}{page.Title}", ImGuiChildFlags.AlwaysUseWindowPadding);
                GuiUtils.PushRounding();
                ConvertDragToScroll();
                page.Renderer();
                GuiUtils.PopRounding();
                ImGui.EndChild();
                
                GuiUtils.PopColorAccents();
                ImGui.PopStyleVar();

                ImGui.EndTabItem();
            }

            ImGui.PopStyleColor();

            i++;
        }

        _restoredSections.Add(sectionIndex);
        ImGui.PopStyleVar();
        ImGui.EndTabBar();
        ImGui.PopStyleColor(3);
        ImGui.EndChild();

        var versionPos = startPos with { X = ImGui.GetWindowWidth() - ImGui.CalcTextSize(Session.Version).X - Constants.GuiItemSpacing.X};
        ImGui.GetWindowDrawList().AddText(versionPos, ImGui.GetColorU32(GuiColor.Gray), Session.Version);
    }
    
    /// <summary>
    /// Because the window should not be draggable inside the GL window, we convert
    /// dragging to scrolling, so that internal surfaces can be moved instead.
    /// </summary>
    private static void ConvertDragToScroll()
    {
        if (
            ImGui.IsPopupOpen("", ImGuiPopupFlags.AnyPopup)
            || !ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows)
            || !ImGui.IsMouseDragging(ImGuiMouseButton.Left)
        ) return;
        
        var io = ImGui.GetIO();
        var delta = io.MouseDelta;
        ImGui.SetScrollX(ImGui.GetScrollX() - delta.X);
        ImGui.SetScrollY(ImGui.GetScrollY() - delta.Y);
    }
}