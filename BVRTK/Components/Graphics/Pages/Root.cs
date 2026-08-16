using System.Diagnostics;
using System.Numerics;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics.Pages;

public static class Root
{
    public static void RenderAboutPage()
    {
        ImGui.Dummy(Vector2.Zero);
        GuiUtils.DrawCenteredImage(Session.GuiImages.Logo);
        GuiUtils.DrawCenteredText("BOLL's VR Toolkit");
        ImGui.Dummy(Vector2.Zero);
        ImGui.Separator();
        ImGui.Text("Links, opens in your default browser.");
        if (ImGui.TextLink("Discord")) GuiUtils.OpenUrl("https://discord.gg/Cdt4xjqV35");
        ImGui.SameLine();
        if (ImGui.TextLink("Github")) GuiUtils.OpenUrl("https://github.com/BOLL7708/BVRTK");
        ImGui.SameLine();
        if (ImGui.TextLink("Website")) GuiUtils.OpenUrl("https://boll.software");
    }

    public static void RenderVersionHistoryPage()
    {
        
    }
}