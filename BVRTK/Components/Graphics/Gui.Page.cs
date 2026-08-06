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
        ImGui.BeginChild($"##Page{section.Title}", Vector2.Zero);
        ImGui.Dummy(Vector2.Zero); // Simply adds the default spacing at the top.
        ImGui.GetStyle().TabBarBorderSize = 0;

        ImGui.PushStyleColor(ImGuiCol.TabSelected, section.AccentColor.TabActive());
        ImGui.PushStyleColor(ImGuiCol.Tab, section.AccentColor.Tab());
        ImGui.PushStyleColor(ImGuiCol.TabDimmed, section.AccentColor.Tab()); // Not really used but set just in case
        if (!ImGui.BeginTabBar($"##Tabs{section.Title}"))
        {
            ImGui.EndChild();
            return;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, Constants.GuiGlobalRounding);
        foreach (var page in section.Pages)
        {
            SelectedTabs.TryGetValue(_selectedSection, out var selectedTab);
            var isActive = selectedTab == i;
            ImGui.PushStyleColor(ImGuiCol.TabHovered, isActive
                ? section.AccentColor.TabActive()
                : section.AccentColor.TabHover()
            );
            ImGui.PushStyleColor(ImGuiCol.Text, isActive
                ? GuiColor.Black
                : GuiColor.White
            );
            var tabExists = ImGui.BeginTabItem(page.Title);
            ImGui.PopStyleColor();
            if (tabExists)
            {
                // Instead of messing with repacing item spacing, we simply move the cursor up the same distance to negate it.
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGui.GetStyle().ItemSpacing.Y);

                // Drawing a custom separation line for the tab bar that matches the sidebar.
                ImGui.PushStyleColor(ImGuiCol.ChildBg, section.AccentColor.TabActive());
                ImGui.BeginChild($"##Separator{section.Title}", Vector2.Zero with { Y = Constants.GuiSeparatorGirth });
                ImGui.EndChild();
                ImGui.PopStyleColor();
                
                // Store that this tab was selected, helps with color states.
                SelectedTabs[_selectedSection] = i;

                // Also skip the automatic spacing below the custom line.
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGui.GetStyle().ItemSpacing.Y);
                
                PushColorAccents(section.AccentColor);
                ImGui.BeginChild($"##Content{section.Title}{page.Title}");
                page.Renderer(section.AccentColor);
                ImGui.EndChild();
                PopColorAccents();

                ImGui.EndTabItem();
            }

            ImGui.PopStyleColor();

            i++;
        }

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
        
        { ImGuiCol.ScrollbarBg, 0.1f},
        { ImGuiCol.ScrollbarGrab, 0.5f},
        { ImGuiCol.ScrollbarGrabHovered, 0.75f},
        { ImGuiCol.ScrollbarGrabActive, 1f},
        
        // Unverified entries below
        { ImGuiCol.SliderGrab, 1f },
        { ImGuiCol.SliderGrabActive, 1.2f },
        { ImGuiCol.FrameBgHovered, 0.5f },
        { ImGuiCol.FrameBgActive, 0.7f },
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
}

