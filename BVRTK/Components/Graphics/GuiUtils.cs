using System.Diagnostics;
using System.Numerics;
using BVRTK.Data;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static class GuiUtils
{
    #region Settings

    public static unsafe void PushFont(FontStyle font, float size = 0)
    {
        switch (font)
        {
            case FontStyle.Bold:
                ImGui.PushFont(Session.GuiFonts.Bold, size);
                break;
            case FontStyle.Italic:
                ImGui.PushFont(Session.GuiFonts.Italic, size);
                break;
            case FontStyle.BoldItalic:
                ImGui.PushFont(Session.GuiFonts.BoldItalic, size);
                break;
            case FontStyle.Regular:
            default:
                ImGui.PushFont(Session.GuiFonts.Regular, size);
                break;
        }
    }

    /// <summary>
    /// Contains all the styles we colorize for the various pages.
    /// Add more styles here when need arises.
    /// </summary>
    private static readonly Dictionary<ImGuiCol, float> AccentComponents = new()
    {
        { ImGuiCol.ChildBg, 0.25f },
        { ImGuiCol.CheckMark, 1f },

        // Used for any element with a scrollbar
        { ImGuiCol.ScrollbarBg, 0.1f },
        { ImGuiCol.ScrollbarGrab, 0.5f },
        { ImGuiCol.ScrollbarGrabHovered, 0.75f },
        { ImGuiCol.ScrollbarGrabActive, 1f },

        // Used for things like the checkmark
        { ImGuiCol.FrameBg, 0.1f },
        { ImGuiCol.FrameBgHovered, 0.5f },
        { ImGuiCol.FrameBgActive, 0.75f },

        // Collapsible header
        { ImGuiCol.Header, 0.3f },
        { ImGuiCol.HeaderHovered, 0.4f },
        { ImGuiCol.HeaderActive, 0.5f },
        
        // Unverified entries below
        { ImGuiCol.SeparatorHovered, 1f },
        { ImGuiCol.SliderGrab, 1f },
        { ImGuiCol.SliderGrabActive, 1.2f },
        { ImGuiCol.Button, 1f },
        { ImGuiCol.ButtonHovered, 1.15f },
        { ImGuiCol.ButtonActive, 0.85f },
        { ImGuiCol.TextSelectedBg, 0.5f },
    };

    public static void PushColorAccents(Vector4 a)
    {
        foreach (var kv in AccentComponents)
        {
            ImGui.PushStyleColor(kv.Key, a.Fade(kv.Value));
        }
    }

    public static void PopColorAccents()
    {
        ImGui.PopStyleColor(AccentComponents.Count);
    }

    #endregion

    #region Draw

    public static void DrawCenteredImage(GlImage image)
    {
        var availableSpace = ImGui.GetContentRegionAvail().X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (availableSpace - image.Width) / 2f);
        image.Draw();
    }

    public static void DrawCenteredText(string text, FontStyle font = FontStyle.Regular, float size = 0)
    {
        PushFont(font, size);
        ImGui.TextAligned(0.5f, ImGui.GetContentRegionAvail().X, text);
        ImGui.PopFont();
    }

    public static void DrawTooltip(string message)
    {
        if (!Settings.Current.Application.ShowTooltips || !ImGui.IsItemHovered()) return;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, Constants.GuiGeneralRounding);
        ImGui.BeginTooltip();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * Constants.GuiTooltipWrap);
        ImGui.TextUnformatted(message);
        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
        ImGui.PopStyleVar();
    }

    private static readonly List<ImGuiStyleVar> RoundingVars = [
        ImGuiStyleVar.WindowRounding,
        ImGuiStyleVar.ChildRounding,
        ImGuiStyleVar.FrameRounding,
        ImGuiStyleVar.PopupRounding,
        ImGuiStyleVar.ScrollbarRounding,
        ImGuiStyleVar.GrabRounding,
        ImGuiStyleVar.TabRounding
    ];
    
    public static void PushRounding()
    {
        foreach (var rv in RoundingVars)
        {
            ImGui.PushStyleVar(rv, Constants.GuiGeneralRounding);
        }        
    }
    
    public static void PopRounding()
    {
        ImGui.PopStyleVar(RoundingVars.Count);
    }

    public static void DrawSeparator(float fade = 0.5f)
    {
        var section = GuiStructure.Sections[Settings.Current.Application.CurrentSection];
        ImGui.PushStyleColor(ImGuiCol.ChildBg, section.AccentColor.Fade(fade));
        ImGui.BeginChild(GetNextSerialTag("HorizontalSeparator"), Vector2.Zero with { Y = Constants.GuiSeparatorGirth });
        ImGui.EndChild();
        ImGui.PopStyleColor();
    }

    #endregion

    private static int _tagSerial = 0;
    public static string GetNextSerialTag(string tag = "SerialTag")
    {
        _tagSerial++;
        return $"##{tag}{_tagSerial}";
    }
    
    #region System

    /// <summary>
    /// Launches the provided URL in the default external browser.
    /// </summary>
    /// <param name="url"></param>
    public static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    #endregion
}