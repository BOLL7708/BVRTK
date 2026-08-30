using System.Globalization;
using System.Numerics;
using BVRTK.Components.KeyboardSimulator;

namespace BVRTK;

public static class Constants
{
    public static readonly string SystemApplicationKey = "software.boll.bvrtk";
    public static readonly string SystemDefaultLanguage = "en-US";
    
    /// Forever-invite that applies the from-bvrtk role on the server.
    public static readonly string UrlDiscordInvite = "https://discord.gg/nuegP5CRXh";
    public static readonly string UrlGithubRepository = "https://github.com/BOLL7708/BVRTK";
    public static readonly string UrlDeveloperWebsite = "https://boll.software/bvrtk";
    
    public static readonly string OverlayUniqueId = "bvrtk.application.window.overlay";
    public static readonly string OverlayTitle = "BVRTK";
    public static readonly int OverlayTextureWidth = 1440;
    public static readonly int OverlayTextureHeight = 960;
    public static readonly float OverlayPhysicalWidth = 2.5f;
    public static readonly float OverlayGuiScale = 2f;

    public static readonly float GuiFontSize = 10f * OverlayGuiScale;
    public static readonly float GuiSidebarWidth = 128f * OverlayGuiScale;
    public static readonly float GuiTabRounding = 8f * OverlayGuiScale;
    public static readonly float GuiGeneralRounding = 4f * OverlayGuiScale;
    public static readonly float GuiMainSeparatorGirth = 6f * OverlayGuiScale;
    public static readonly Vector2 GuiItemSpacing = new (8f * OverlayGuiScale, 6f * OverlayGuiScale);
    public static readonly float GuiTooltipWrap = 16f;
    public static readonly float GuiSeparatorGirth = 3f * OverlayGuiScale;
    public static readonly float GuiBorderWidth = 1.5f * OverlayGuiScale;

    public static readonly string[] KeyboardSimulatorKeyCodeGuiTags = KeyboardSimulatorUtils.GetGuiTags();

    public static readonly Dictionary<string, CultureInfo> SupportedLanguages = new()
    {
        { "en-US", CultureInfo.InvariantCulture },
        { "sv-SE", new CultureInfo("sv-SE") },
    };

    public static readonly string[] SupportedLanguageGuiTags = Utils.GetSupportedLanguageGuiTags();
}