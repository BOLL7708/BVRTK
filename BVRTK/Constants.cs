using System.Globalization;
using System.Numerics;
using BVRTK.Components.KeyboardSimulator;

namespace BVRTK;

public static class Constants
{
    public const string SystemApplicationKey = "steam.app.4231070";
    public const string SystemDefaultLanguage = "en-US";

    public static class Url
    {
        /// Forever-invite that applies the from-bvrtk role on the server.
        public const string DiscordInvite = "https://discord.gg/nuegP5CRXh";
        public const string GithubRepository = "https://github.com/BOLL7708/BVRTK";
        public const string DeveloperWebsite = "https://boll.software/bvrtk";
    }

    public static class Overlay
    {
        public const string UniqueId = "bvrtk.application.window.overlay";
        public const string Title = "BVRTK";
        public const int TextureWidth = 1440;
        public const int TextureHeight = 960;
        public const float PhysicalWidth = 2.5f;
        public const float GuiScale = 2f;
    }

    public static class Gui
    {
        
        public const float FontSize = 10f * Overlay.GuiScale;
        public const float SidebarWidth = 128f * Overlay.GuiScale;
        public const float TabRounding = 8f * Overlay.GuiScale;
        public const float GeneralRounding = 4f * Overlay.GuiScale;
        public const float MainSeparatorGirth = 6f * Overlay.GuiScale;
        public const float TooltipWrap = 16f;
        public const float SeparatorGirth = 3f * Overlay.GuiScale;
        public const float BorderWidth = 1.5f * Overlay.GuiScale;
    }
    
    public static readonly Vector2 GuiItemSpacing = new (8f * Overlay.GuiScale, 6f * Overlay.GuiScale);

    public static readonly Dictionary<string, CultureInfo> SupportedLanguages = new()
    {
        { "en-US", CultureInfo.InvariantCulture },
        { "sv-SE", new CultureInfo("sv-SE") },
    };

    public static class ActionSet
    {
        public const string Default = "default";
        public const string KeyboardSim = "keyboardsim";
    }
}