using System.Diagnostics;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static class GuiUtils
{
    public static unsafe void PushFont(FontStyle font)
    {
        switch (font)
        {
            case FontStyle.Bold:
                ImGui.PushFont(Session.GuiFonts.Bold, 0f);
                break;
            case FontStyle.Italic:
                ImGui.PushFont(Session.GuiFonts.Italic, 0f);
                break;
            case FontStyle.BoldItalic:
                ImGui.PushFont(Session.GuiFonts.BoldItalic, 0f);
                break;
            case FontStyle.Regular:
            default:
                ImGui.PushFont(Session.GuiFonts.Regular, 0f);
                break;
        }
    }

    public static void DrawCenteredImage(GlImage image)
    {
        var availableSpace = ImGui.GetContentRegionAvail().X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (availableSpace - image.Width) / 2f);
        image.Draw();
    }

    public static void DrawCenteredText(string text, FontStyle font = FontStyle.Regular)
    {
        PushFont(font);
        ImGui.TextAligned(0.5f, ImGui.GetContentRegionAvail().X, text);
        ImGui.PopFont();
    }

    public static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}