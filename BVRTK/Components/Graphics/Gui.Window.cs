using System.Numerics;
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
        // ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);

        const ImGuiWindowFlags flags =
            ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;

        #endregion

        #region Draw

        ImGui.Begin("##Root", flags);
        // ImGui.PopStyleColor();
        ImGui.PopStyleVar(3);
        ConvertWindowMovementToScrolling();

        RenderSidebar();
        ImGui.SameLine(0, 0);
        RenderSeparator();
        ImGui.SameLine();
        RenderPage();

        ImGui.End();

        #endregion
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