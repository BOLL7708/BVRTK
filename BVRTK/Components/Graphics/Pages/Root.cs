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
    }

    public static void RenderLinksPage()
    {
        
    }

    public static void RenderVersionHistoryPage()
    {
        
    }
}