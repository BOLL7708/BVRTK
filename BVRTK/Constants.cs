using System.Numerics;

namespace BVRTK;

public static class Constants
{
    public static readonly string OverlayUniqueId = "bvrtk.application.window.overlay";
    public static readonly string OverlayTitle = "BVRTK";
    public static readonly int OverlayTextureWidth = 1440;
    public static readonly int OverlayTextureHeight = 960;
    public static readonly float OverlayPhysicalWidth = 2.5f;
    public static readonly float OverlayGuiScale = 2.5f;

    public static readonly float GuiSidebarWidth = 128f * OverlayGuiScale;
    public static readonly float GuiGlobalRounding = 8f * OverlayGuiScale;
    public static readonly float GuiSeparatorGirth = 8f * OverlayGuiScale;
    public static readonly Vector2 GuiItemSpacing = new Vector2(8f * OverlayGuiScale, 6f * OverlayGuiScale);
}