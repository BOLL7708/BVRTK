using System.Numerics;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static class GuiColor
{
    public static readonly Vector4 White = Vector4.One;
    public static readonly Vector4 Black = Vector4.Zero with { W = 1f };
    public static readonly Vector4 GrayLightestest = new(0.9f, 0.9f, 0.9f, 1f);
    public static readonly Vector4 GrayLightest = new(0.8f, 0.8f, 0.8f, 1f);
    public static readonly Vector4 GrayLighter = new(0.7f, 0.7f, 0.7f, 1f);
    public static readonly Vector4 GrayLight = new(0.6f, 0.6f, 0.6f, 1f);
    public static readonly Vector4 Gray = new(0.5f, 0.5f, 0.5f, 1f);
    public static readonly Vector4 GrayDark = new(0.4f, 0.4f, 0.4f, 1f);
    public static readonly Vector4 GrayDarker = new(0.3f, 0.3f, 0.3f, 1f);
    public static readonly Vector4 GrayDarkest = new(0.2f, 0.2f, 0.2f, 1f);
    public static readonly Vector4 GrayDarkestest = new(0.1f, 0.1f, 0.1f, 1f);

    public static readonly Vector4 Root = new Vector4(0.8f, 0.8f, 0.8f, 1f);
    public static readonly Vector4 Preferences = Gray;
    public static readonly Vector4 Server = new Vector4(0.9f, 0.4f, 0.2f, 1f);
    public static readonly Vector4 KeyboardSim = new Vector4(0f, 0f, 1f, 1f);
    public static readonly Vector4 MouseSim = Gray;
    public static readonly Vector4 Screenshots = new Vector4(1f, 0.8f, 0f, 1f);
    public static readonly Vector4 Overlays = new Vector4(1f, 0.4f, 1f, 1f);
    

    public static Vector4 FromHue(float hue)
    {
        float r = 0, g = 0, b = 0;
        ImGui.ColorConvertHSVtoRGB(hue, 1f, 1f, ref r, ref g, ref b);
        return new Vector4(r, g, b, 1f);
    }
}

public static class Vector4Extensions
{
    public static Vector4 Fade(this Vector4 vec, float value) => (vec * value) with { W = 1f };

    public static Vector4 Tab(this Vector4 vec) => vec.Fade(0.5f);
    public static Vector4 TabHover(this Vector4 vec) => vec.Fade(0.75f);
    public static Vector4 TabActive(this Vector4 vec) => vec;
    
    public static Vector4 PageBg(this Vector4 vec) => vec.Fade(0.2f);
}