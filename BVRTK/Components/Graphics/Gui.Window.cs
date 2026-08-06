using System.Drawing;
using System.Numerics;
using BVRTK.Data;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static partial class Gui
{
    public static void RenderWindow()
    {
        #region Setup

        // Full screen
        ImGuiViewportPtr vp = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(vp.Pos);
        ImGui.SetNextWindowSize(vp.Size);

        // Window styling
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, GuiColor.Black);

        const ImGuiWindowFlags flags =
            ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;

        #endregion

        #region Draw

        ImGui.Begin("##Root", flags);
        ImGui.PopStyleColor();
        ImGui.PopStyleVar(3);

        ConvertWindowMovementToScrolling();
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Constants.GuiItemSpacing);
        RenderSidebar();
        ImGui.SameLine(0, 0);
        RenderSeparator();
        ImGui.SameLine();
        RenderPage();
        if (Settings.Current.Application.EnableInterfaceGradient) RenderGradient();
        ImGui.PopStyleVar();
        ImGui.End();

        #endregion
    }

    private static void RenderGradient()
    {
        var drawList = ImGui.GetForegroundDrawList();
        var top = ImGui.ColorConvertFloat4ToU32(GuiColor.White with { W = 0.2f });
        var middle = ImGui.ColorConvertFloat4ToU32(GuiColor.Gray with { W = 0 });
        var bottom = ImGui.ColorConvertFloat4ToU32(GuiColor.Black with { W = 0.2f });
        drawList.AddRectFilledMultiColor(
            Vector2.Zero, ImGui.GetWindowSize(),
            top, middle, bottom, middle
        );
    }

    /// <summary>
    /// Because the window should not be draggable inside the GL window, we convert
    /// dragging to scrolling, so that internal surfaces can be moved instead.
    /// </summary>
    private static void ConvertWindowMovementToScrolling()
    {
        var io = ImGui.GetIO();
        if (
            ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows)
            && !ImGui.IsAnyItemActive()
            && ImGui.IsMouseDragging(ImGuiMouseButton.Left)
        )
        {
            var delta = io.MouseDelta;
            ImGui.SetScrollX(ImGui.GetScrollX() - delta.X);
            ImGui.SetScrollY(ImGui.GetScrollY() - delta.Y);
        }
    }
}