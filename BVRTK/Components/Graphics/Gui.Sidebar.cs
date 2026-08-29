using System.Numerics;
using BVRTK.Data;
using Hexa.NET.ImGui;
using Valve.VR;

namespace BVRTK.Components.Graphics;

public static partial class Gui
{
    private static void RenderSidebar()
    {
        // The sidebar
        ImGui.BeginChild("##Sidebar", new Vector2(Constants.GuiSidebarWidth, 0));

        // To add top space.
        ImGui.Dummy(Vector2.Zero);

        #region Tabs

        // Section buttons, each colored by its accent
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, Constants.GuiTabRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, ImGui.GetStyle().FramePadding with { X = Constants.GuiTabRounding * 2f });
        ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(1f, 0.5f)); // Horizontal and vertical alignment

        var availableSpace = ImGui.GetContentRegionAvail();
        var i = 0;
        foreach (var section in GuiStructure.Sections)
        {
            if (!Session.isDebug && !section.IsPublic)
            {
                i++;
                continue;
            }

            GuiUtils.PushFont(section.Font);
            var isActive = i == Settings.Current.Application.CurrentSection;
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
                ? GuiColor.Black
                : GuiColor.White
            );

            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Constants.GuiTabRounding);
            ImGui.Button(section.Title, availableSpace with { Y = 0 });
            if (ImGui.IsItemActivated())
            {
                Settings.Current.Application.CurrentSection = i;
            }

            ImGui.PopStyleColor(4);
            ImGui.PopFont();
            i++;
        }

        ImGui.PopStyleVar(3);

        #endregion

        #region Quick Settings

        GuiUtils.PushColorAccents(GuiColor.Root);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, Vector4.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, Constants.GuiGeneralRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Constants.GuiItemSpacing);
        ImGui.BeginChild("##QuickSettings", ImGuiChildFlags.AlwaysUseWindowPadding);

        var showTooltips = Settings.Current.Application.ShowTooltips;
        if (ImGui.Checkbox("Tooltips", ref showTooltips))
        {
            Settings.Current.Application.ShowTooltips = showTooltips;
        }

        GuiUtils.DrawTooltip("Toggle tooltips for all places where a tooltip exists.");

        var showOnDesktop = Services.GuiBackend.IsWindowVisible();
        if (ImGui.Checkbox("On Desktop", ref showOnDesktop))
        {
            Services.GuiBackend.SetWindowVisible(showOnDesktop);
        }

        GuiUtils.DrawTooltip("Display this application in a mirror window on the desktop.");

        var buttonSize = (availableSpace - Constants.GuiItemSpacing * 2) with { Y = 0 };
        ImGui.PushStyleColor(ImGuiCol.Button, GuiColor.Transparent);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, GuiColor.White with { W = 0.25f });
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, GuiColor.White with { W = 0.5f });

        if (ImGui.Button("Edit Bindings", buttonSize))
        {
            var steamVrError = OpenVR.Input.OpenBindingUI("", 0, 0, !Session.OverlayFocus && Session.DesktopFocus);
            // TODO: Log error to logging output, possible pop an error modal?
        }
        GuiUtils.DrawTooltip("Launch or switch to the SteamVR bindings editor for this application.");
        
        if (ImGui.Button("Exit", buttonSize))
        {
            // TODO: Add modal confirmation dialog for this!
            Session.ExitPressed = true;
            Session.ProgramCts.Cancel();
        }

        ImGui.PopStyleColor(3);

        #region Debug

        // ImGui.TextColored(new Vector4(1f, 0, 0, 1f), $"{string.Join(Environment.NewLine, Settings.Current.Application.CurrentSection)}");

        #endregion

        ImGui.EndChild();
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor();
        GuiUtils.PopColorAccents();

        #endregion

        ImGui.EndChild();
    }
}