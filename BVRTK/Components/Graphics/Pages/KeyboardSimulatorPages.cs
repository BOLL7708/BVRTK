using Hexa.NET.ImGui;
using Steamworks;

namespace BVRTK.Components.Graphics.Pages;

public static class KeyboardSimulatorPages
{
    public static void Entries()
    {
        GuiUtils.DrawTitle("Hello There");
        foreach (var entry in Session.GuiActionEntries)
        {
            ImGui.Text($"{entry.Path} => {entry.Prompt()}");
        }
    }
}