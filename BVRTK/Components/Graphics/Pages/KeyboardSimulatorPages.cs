using BVRTK.Data;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics.Pages;

public static class KeyboardSimulatorPages
{
    public static void Entries()
    {
        GuiUtils.DoModal<string>(
            "keyboard-simulator-general",
            "Add general entry",
            "Add", "Abort",
            "", // Empty as we start from nothing
            (value) =>
            {
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted("For use regardless of which game is running:");
            // No additional GUI needed as we are just displaying the add button.
            }, 
            RenderDialog,
            (value) =>
            {
                Settings.Current.KeyboardSimulator.InternalEntriesGeneralAdd(value);
            }
        );
        
        GuiUtils.DrawTitle("General Entries");
        // TODO: Display a list of added entries, this should extend the more we add.
        //  * Description [Edit]
        
        foreach(var universalEntry in Settings.Current.KeyboardSimulator.EntriesGeneral)
        {
            ImGui.TextUnformatted(universalEntry);
        }
        
        // foreach (var key in KeyboardSimulatorUtils.GetGuiTags())
        // {
        //     ImGui.Text($"Key tag: {key}");
        // }
        // foreach (var entry in Session.GuiActionEntries)
        // {
        //     ImGui.Text($"Input tag: {entry.Tag}");
        // }
        
        
        GuiUtils.DrawTitle("Game Specific Entries");
        
    }

    private static string RenderDialog(string startValue)
    {
        var result = startValue;
        ImGui.InputText("keyboard-simulator", ref result, 128);
        return result;
    }
}