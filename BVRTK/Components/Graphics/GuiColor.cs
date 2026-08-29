using System.Numerics;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static class GuiColor
{
    public static readonly Vector4 Transparent = Vector4.Zero;
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

    public static readonly Vector4 Root = new(0.8f, 0.8f, 0.8f, 1f);
    public static readonly Vector4 Preferences = new(0.478f, 0.435f, 0.941f, 1f);
    public static readonly Vector4 Server = new(0.910f, 0.333f, 0.176f, 1f);
    public static readonly Vector4 KeyboardSim = new(0.357f, 0.753f, 0.922f, 1f);
    public static readonly Vector4 MouseSim = new(0.263f, 0.753f, 0.349f, 1f);
    public static readonly Vector4 Overlays = new(0.910f, 0.361f, 0.565f, 1f);
    public static readonly Vector4 Screenshots = new(0.961f, 0.773f, 0.094f, 1f);
    public static readonly Vector4 PlayArea = new(0.090f, 0.635f, 0.627f, 1f);
    public static readonly Vector4 Events = new(0.659f, 0.341f, 0.902f, 1f);

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
}