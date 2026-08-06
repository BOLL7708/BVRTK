using System.Numerics;
using BVRTK.Data;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public static partial class Gui
{
    public static void RenderPageSettings(Vector4 color)
    {
        var gradients = Settings.Current.Application.EnableInterfaceGradient;
        if (ImGui.Checkbox("Enable interface gradient", ref gradients))
        {
            Settings.Current.Application.EnableInterfaceGradient = gradients;
        }
    }
}