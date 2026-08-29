using System.Numerics;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static partial class Gui
{
    private static void RenderExitPage()
    {
        const string text = "Shutting down...";
        var available = ImGui.GetContentRegionAvail();
        ImGui.SetCursorPos(available * 0.5f - ImGui.CalcTextSize(text) * 0.5f);
        ImGui.TextUnformatted(text);
    }
}