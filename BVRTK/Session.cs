using System.Numerics;
using BVRTK.Components.Graphics;
using Hexa.NET.ImGui;

namespace BVRTK;

public static class Session
{
#if DEBUG
    public const bool isDebug = true;
#else
    public const bool isDebug = false;
#endif

    public static string Version { get; set; } = "v0.0.0";

    public static unsafe class GuiFonts
    {
        public static ImFont* Regular { get; set; }
        public static ImFont* Bold { get; set; }
        public static ImFont* Italic { get; set; }
        public static ImFont* BoldItalic { get; set; }
    }

    public static class GuiImages
    {
        public static GlImage Logo;
    }
    
    public static readonly CancellationTokenSource ProgramCts = new();

    public static bool ExitPressed { get; set; }
    
    public static bool OverlayFocus { get; set; }
    public static bool DesktopFocus { get; set; }
}