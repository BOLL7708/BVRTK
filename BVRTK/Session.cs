using Hexa.NET.ImGui;

namespace BVRTK;

public static class Session
{
#if DEBUG
    public const bool isDebug = true;
#else
    public const bool isDebug = false;
#endif
    public static class GuiFonts
    {
        public static ImFontPtr Regular;
        public static ImFontPtr Bold;
        public static ImFontPtr Italic;
        public static ImFontPtr BoldItalic;
    }
}