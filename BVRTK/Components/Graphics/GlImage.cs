using System.Numerics;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics;

public unsafe class GlImage(uint ptr, int width, int height)
{
    public readonly int Width = width;
    public readonly int Height = height;
    
    public ImTextureRef ToTextureRef()
    {
        return new ImTextureRef(texId: new ImTextureID((nint)ptr));
    }

    public void Draw(float scale = 1f)
    {
        ImGui.Image(ToTextureRef(), new Vector2(Width * scale, Height * scale));
    }
}