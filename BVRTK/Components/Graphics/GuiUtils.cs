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
        { ImGuiCol.CheckMark, -1f },

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

        // Buttons
        { ImGuiCol.Button, 0.5f },
        { ImGuiCol.ButtonHovered, 0.75f },
        { ImGuiCol.ButtonActive, 1f },

        // Popups
        { ImGuiCol.PopupBg, 0.25f },
        { ImGuiCol.TitleBg, 0.75f },
        { ImGuiCol.TitleBgActive, 1f },
        { ImGuiCol.TitleBgCollapsed, 0.5f },
        { ImGuiCol.Border, 1f },

        // Unverified entries below
        { ImGuiCol.SeparatorHovered, 1f },
        { ImGuiCol.SliderGrab, 1f },
        { ImGuiCol.SliderGrabActive, 1.2f },
        { ImGuiCol.TextSelectedBg, 0.5f },
    };

    public static void PushColorAccents(Vector4 a)
    {
        foreach (var kv in AccentComponents)
        {
            ImGui.PushStyleColor(kv.Key, kv.Value <= 0 ? GuiColor.White : a.Fade(kv.Value));
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

    public static void DrawRightAlignedText(string text, FontStyle font = FontStyle.Regular, float size = 0)
    {
        PushFont(font, size);
        ImGui.TextAligned(1f, ImGui.GetContentRegionAvail().X, text);
        ImGui.PopFont();
    }

    public static void DrawText(string text, FontStyle font = FontStyle.Regular, float size = 0)
    {
        PushFont(font, size);
        ImGui.Text(text);
        ImGui.PopFont();
    }

    public static void DrawTitle(string title)
    {
        ImGui.Dummy(Vector2.Zero);
        DrawCenteredText(title, FontStyle.Bold, Constants.Gui.FontSize * 1.25f);
        DrawDivider();
    }

    #region Modals

    private static object? _modalDialogValue = null;

    /// <summary>
    /// Render optional interface and a button to open the modal.
    /// Will run the preparatory step once on button click if provided.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="button"></param>
    /// <param name="prepare"></param>
    /// <param name="renderGui"></param>
    /// <param name="startValue"></param>
    /// <typeparam name="T"></typeparam>
    public static void OpenModal<T>(
        string tag,
        string button,
        Action? prepare,
        Action<T>? renderGui,
        T startValue
    )
    {
        if (renderGui != null)
        {
            renderGui(startValue);
            ImGui.SameLine();
        }

        var open = ImGui.Button($"{button}##{tag}Button");

        if (!open) return;

        prepare?.Invoke();
        ImGui.OpenPopup(tag);
    }

    /// <summary>
    /// The modal itself, : renders a custom interface and outputs the result.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="okButtonLabel"></param>
    /// <param name="cancelButtonLabel"></param>
    /// <param name="renderDialogGui"></param>
    /// <param name="startValue"></param>
    /// <param name="updateSetting"></param>
    /// <typeparam name="T"></typeparam>
    public static void DrawModal<T>(
        string tag,
        string okButtonLabel,
        string cancelButtonLabel,
        Func<T, T>? renderDialogGui,
        T startValue,
        Action<T> updateSetting
    )
    {
        var vp = ImGui.GetMainViewport();
        var center = vp.Pos + (vp.Size + new Vector2(Constants.Gui.SidebarWidth + Constants.Gui.MainSeparatorGirth, 0)) * 0.5f;
        var buttonSize = new Vector2(128f * Constants.Overlay.GuiScale, 0);

        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.PushStyleColor(ImGuiCol.ModalWindowDimBg, GuiColor.Black with { W = 0.5f });
        ImGui.SetNextWindowSizeConstraints((vp.Size * 0.5f) with { Y = 0 }, vp.Size);

        ImGui.PushStyleColor(ImGuiCol.Text, GuiColor.Black);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, Constants.Gui.BorderWidth);
        if (ImGui.BeginPopupModal(tag, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, GuiColor.White);
            if (ImGui.IsWindowAppearing()) _modalDialogValue = startValue;

            var temp = (T)_modalDialogValue!;
            if (renderDialogGui != null) temp = renderDialogGui(temp);
            _modalDialogValue = temp;

            var popupWidth = ImGui.GetContentRegionAvail().X;

            var enter = ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsKeyPressed(ImGuiKey.KeypadEnter);
            if (ImGui.Button(okButtonLabel, buttonSize) || enter)
            {
                updateSetting(temp);
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            ImGui.SetCursorPosX(popupWidth - buttonSize.X + Constants.GuiItemSpacing.X);
            var escape = ImGui.IsKeyPressed(ImGuiKey.Escape);
            if (ImGui.Button(cancelButtonLabel, buttonSize) || escape) ImGui.CloseCurrentPopup();

            ImGui.PopStyleColor();
            ImGui.EndPopup();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(2);
    }

    /// <summary>
    /// The full modal  function that renders the GUI that will launch the model dialog,
    /// and the modal dialog itself when triggered.
    /// Will also run a preparation step if it was provided, as well as an update step.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="launchButtonLabel"></param>
    /// <param name="okButtonLabel"></param>
    /// <param name="cancelButtonLabel"></param>
    /// <param name="startValue"></param>
    /// <param name="prepareSetting"></param>
    /// <param name="renderGui"></param>
    /// <param name="renderDialogGui"></param>
    /// <param name="updateSetting"></param>
    /// <typeparam name="T"></typeparam>
    public static void DoModal<T>(
        string tag,
        string launchButtonLabel,
        string okButtonLabel,
        string cancelButtonLabel,
        T startValue,
        Action? prepareSetting,
        Action<T>? renderGui,
        Func<T, T>? renderDialogGui,
        Action<T> updateSetting)
    {
        OpenModal(tag, launchButtonLabel, prepareSetting, renderGui, startValue);
        DrawModal(tag, okButtonLabel, cancelButtonLabel, renderDialogGui, startValue, updateSetting);
    }

    public static void DoModalForInt(string tag, string label, string button, float size, int startValue, Action<int> updateSetting)
    {
        DoModal(tag, button, "Apply", "Cancel", startValue,
            null,
            value =>
            {
                ImGui.SetNextItemWidth(size * Constants.Overlay.GuiScale);
                ImGui.InputInt(label, ref value, 0, ImGuiInputTextFlags.ReadOnly);
            },
            value =>
            {
                if (ImGui.IsWindowAppearing()) ImGui.SetKeyboardFocusHere(0);
                ImGui.SetNextItemWidth(size * Constants.Overlay.GuiScale);
                ImGui.InputInt(label, ref value, 0, ImGuiInputTextFlags.CharsDecimal);
                return value;
            }, updateSetting);
    }

    public static void DoModalForString(string tag, string label, string button, float size, string startValue, uint maxLength, Action<string> updateSetting)
    {
        DoModal(tag, button, "Apply", "Cancel", startValue,
            null,
            value =>
            {
                ImGui.SetNextItemWidth(size * Constants.Overlay.GuiScale);
                ImGui.InputText(label, ref value, maxLength, ImGuiInputTextFlags.ReadOnly);
            },
            value =>
            {
                if (ImGui.IsWindowAppearing()) ImGui.SetKeyboardFocusHere(0);
                ImGui.SetNextItemWidth(size * Constants.Overlay.GuiScale);
                ImGui.InputText(label, ref value, maxLength, ImGuiInputTextFlags.None);
                return value;
            }, updateSetting);
    }

    public static void DoModalToConfirm(
        string tag,
        string button,
        string message,
        string positiveButton,
        string negativeButton,
        Action onConfirmed
    )
    {
        DoModal(
            tag,
            button,
            positiveButton,
            negativeButton,
            false,
            null,
            null,
            message.IsWhiteSpace() ? null : value =>
            {
                ImGui.Text(message);
                return value;
            },
            _ => onConfirmed()
        );
    }

    #endregion

    public static void DrawTooltip(string message)
    {
        if (
            !Settings.Current.Application.ShowTooltips
            || !ImGui.IsItemHovered()
            || string.IsNullOrWhiteSpace(message)
        ) return;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, Constants.Gui.GeneralRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, Constants.Gui.BorderWidth);
        ImGui.BeginTooltip();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * Constants.Gui.TooltipWrap);
        ImGui.TextUnformatted(message);
        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
        ImGui.PopStyleVar(2);
    }

    private static readonly List<ImGuiStyleVar> RoundingVars =
    [
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
            ImGui.PushStyleVar(rv, Constants.Gui.GeneralRounding);
        }
    }

    public static void PopRounding()
    {
        ImGui.PopStyleVar(RoundingVars.Count);
    }

    public static void DrawDivider(float fade = 0.5f)
    {
        var section = GuiStructure.Sections[Settings.Current.Application.CurrentSection];
        var color = ImGui.ColorConvertFloat4ToU32(section.AccentColor.Fade(fade));
        var pos = ImGui.GetCursorScreenPos();
        var size = new Vector2(ImGui.GetContentRegionAvail().X, Constants.Gui.SeparatorGirth);
        ImGui.GetWindowDrawList().AddRectFilled(pos, pos + size, color, Constants.Gui.GeneralRounding);
        ImGui.Dummy(size);
    }

    #endregion

    private static int _tagSerial = 0;

    public static string GetNextSerialTag(string tag = "SerialTag")
    {
        _tagSerial++;
        var generated = $"##{tag}{_tagSerial}";
        Console.WriteLine($"Generated Gui Tag: {generated}, if this message spams, you are using GetNextSerialTag wrong.");
        return generated;
    }

    public static int GetIndexOfTagInIds(string[] ids, string tag)
    {
        return Array.FindIndex(ids, id => GetTagFromId(id) == tag);
    }

    public static string GetTagFromId(string id)
    {
        return id.Split("##", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last();
    }

    public static string GetLabelFromId(string id)
    {
        return id.Split("##", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).First();
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