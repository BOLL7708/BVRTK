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
        ImGui.Dummy(Vector2.Zero); // Simply adds the default spacing at the top.
        ImGui.GetStyle().TabBarBorderSize = 0;

        ImGui.PushStyleColor(ImGuiCol.TabSelected, section.AccentColor.TabActive());
        ImGui.PushStyleColor(ImGuiCol.Tab, section.AccentColor.Tab());
        ImGui.PushStyleColor(ImGuiCol.TabDimmed, section.AccentColor.Tab()); // Not really used but set just in case
        
        ImGui.BeginTabBar($"##Tabs{section.Title}");

        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, Constants.GuiGlobalRounding);
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
                ImGui.BeginChild($"##Separator{section.Title}", Vector2.Zero with { Y = Constants.GuiSeparatorGirth });
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
                PushColorAccents(section.AccentColor);
                
                // Turns out window padding is ignored by default without a border.
                ImGui.BeginChild($"##Content{section.Title}{page.Title}", ImGuiChildFlags.AlwaysUseWindowPadding);
                ConvertDragToScroll();
                page.Renderer();
                ImGui.EndChild();
                
                PopColorAccents();
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
    }

    private static readonly Dictionary<ImGuiCol, float> AccentComponents = new()
    {
        { ImGuiCol.ChildBg, 0.25f },
        { ImGuiCol.CheckMark, 1f },
        // TODO: Checkmark backgrounds are still blue when not focused. 

        // Used for any element with a scrollbar
        { ImGuiCol.ScrollbarBg, 0.1f },
        { ImGuiCol.ScrollbarGrab, 0.5f },
        { ImGuiCol.ScrollbarGrabHovered, 0.75f },
        { ImGuiCol.ScrollbarGrabActive, 1f },

        // Used for things like the checkmark
        { ImGuiCol.FrameBg, 0.1f },
        { ImGuiCol.FrameBgHovered, 0.5f },
        { ImGuiCol.FrameBgActive, 0.75f },

        // Unverified entries below
        { ImGuiCol.SliderGrab, 1f },
        { ImGuiCol.SliderGrabActive, 1.2f },
        { ImGuiCol.Button, 1f },
        { ImGuiCol.ButtonHovered, 1.15f },
        { ImGuiCol.ButtonActive, 0.85f },
        { ImGuiCol.Header, 1f },
        { ImGuiCol.HeaderHovered, 1.15f },
        { ImGuiCol.HeaderActive, 0.85f },
        { ImGuiCol.SeparatorHovered, 1f },
        { ImGuiCol.TextSelectedBg, 0.5f },
    };

    private static void PushColorAccents(Vector4 a)
    {
        foreach (var kv in AccentComponents)
        {
            ImGui.PushStyleColor(kv.Key, a.Fade(kv.Value));
        }
    }

    private static void PopColorAccents()
    {
        ImGui.PopStyleColor(AccentComponents.Count);
    }
    
    /// <summary>
    /// Because the window should not be draggable inside the GL window, we convert
    /// dragging to scrolling, so that internal surfaces can be moved instead.
    /// </summary>
    private static void ConvertDragToScroll()
    {
        if (
            !ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows)
            || !ImGui.IsMouseDragging(ImGuiMouseButton.Left)
        ) return;
        
        var io = ImGui.GetIO();
        var delta = io.MouseDelta;
        ImGui.SetScrollX(ImGui.GetScrollX() - delta.X);
        ImGui.SetScrollY(ImGui.GetScrollY() - delta.Y);
    }
}