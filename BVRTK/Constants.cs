using System.Globalization;
using System.Numerics;
using BVRTK.Components.KeyboardSimulator;

namespace BVRTK;

public static class Constants
{
    public const string SystemApplicationKey = "steam.app.4231070";
    public const string SystemDefaultLanguage = "en-US";

    /// Forever-invite that applies the from-bvrtk role on the server.
    public const string UrlDiscordInvite = "https://discord.gg/nuegP5CRXh";
    public const string UrlGithubRepository = "https://github.com/BOLL7708/BVRTK";
    public const string UrlDeveloperWebsite = "https://boll.software/bvrtk";

    public const string OverlayUniqueId = "bvrtk.application.window.overlay";
    public const string OverlayTitle = "BVRTK";
    public const int OverlayTextureWidth = 1440;
    public const int OverlayTextureHeight = 960;
    public const float OverlayPhysicalWidth = 2.5f;
    public const float OverlayGuiScale = 2f;

    public const float GuiFontSize = 10f * OverlayGuiScale;
    public const float GuiSidebarWidth = 128f * OverlayGuiScale;
    public const float GuiTabRounding = 8f * OverlayGuiScale;
    public const float GuiGeneralRounding = 4f * OverlayGuiScale;
    public const float GuiMainSeparatorGirth = 6f * OverlayGuiScale;
    public const float GuiTooltipWrap = 16f;
    public const float GuiSeparatorGirth = 3f * OverlayGuiScale;
    public const float GuiBorderWidth = 1.5f * OverlayGuiScale;
    
    public static readonly Vector2 GuiItemSpacing = new (8f * OverlayGuiScale, 6f * OverlayGuiScale);

    public static readonly Dictionary<string, CultureInfo> SupportedLanguages = new()
    {
        { "en-US", CultureInfo.InvariantCulture },
        { "sv-SE", new CultureInfo("sv-SE") },
    };
}