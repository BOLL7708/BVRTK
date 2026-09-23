using System.Collections.Immutable;
using System.Globalization;
using System.Numerics;
using System.Text;
using BVRTK.Data;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics.Pages;

public static class KeyboardSimulatorPages
{
    public static void Entries()
    {
        GuiUtils.DrawTitle("General Entries");

        GuiUtils.DoModal<string>(
            "Add General Keyboard Simulation Entry",
            "Add general entry",
            "Add", "Cancel",
            "", // Empty as we start from nothing
            () =>
            {
                ParseEntry(""); // Will reset the entry values.
            },
            (value) =>
            {
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted("For use regardless of which game is running:");
                // No additional GUI needed as we are just displaying the add button.
            },
            RenderDialog,
            value => Settings.Current.KeyboardSimulator.AddIfNewToEntriesGeneral(GuiUtils.GetTagFromId(value))
        );

        var generalIndex = 0;
        RenderList(
            "general",
            [.. Settings.Current.KeyboardSimulator.EntriesGeneral],
            ref generalIndex,
            Settings.Current.KeyboardSimulator.AddIfNewToEntriesGeneral,
            Settings.Current.KeyboardSimulator.RemoveFromEntriesGeneral
        );

        if (Session.SteamSceneAppId.Length > 0)
        {
            GuiUtils.DrawTitle($"Entries for: {Session.SteamSceneAppName}");

            GuiUtils.DoModal<string>(
                "Add Game Specific Keyboard Simulation Entry",
                "Add game specific entry",
                "Add", "Cancel",
                "", // Empty as we start from nothing
                null,
                (value) =>
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted("For use regardless of which game is running:");
                    // No additional GUI needed as we are just displaying the add button.
                },
                RenderDialog,
                AddGameEntry
            );

            // TODO: A list of editable and deletable items that can be reordered, base it on general entries.
            var gameIndex = 0;
            Settings.Current.KeyboardSimulator.EntriesPerGame.TryGetValue(Session.SteamSceneAppId, out var entries);
            if (entries != null)
                RenderList(
                    "game",
                    entries,
                    ref gameIndex,
                    AddGameEntry,
                    RemoveGameEntry
                );
        }
        else
        {
            GuiUtils.DrawTitle("Game Specific Entries");
            GuiUtils.DrawCenteredText("No game detected.");
        }
    }

    private static void AddGameEntry(string label)
    {
        var entry = GuiUtils.GetTagFromId(label);
        Settings.Current.KeyboardSimulator.EntriesPerGame.TryGetValue(Session.SteamSceneAppId, out var existingItems);
        if (existingItems != null && existingItems.Contains(entry)) return;
        var list = existingItems ?? [];
        Settings.Current.KeyboardSimulator.SetInEntriesPerGame(Session.SteamSceneAppId, [.. list, entry]);
    }

    private static void RemoveGameEntry(string label)
    {
        var entry = GuiUtils.GetTagFromId(label);
        Settings.Current.KeyboardSimulator.EntriesPerGame.TryGetValue(Session.SteamSceneAppId, out var existingItems);
        if (existingItems == null || !existingItems.Contains(entry)) return;
        var list = existingItems.ToList();
        list.Remove(entry);
        Settings.Current.KeyboardSimulator.SetInEntriesPerGame(Session.SteamSceneAppId, [.. list, entry]);
    }

    private static readonly string TableTag = GuiUtils.GetNextSerialTag("KeyboardSimulatorModalDialogTable");
    private static readonly string SubTableTag = GuiUtils.GetNextSerialTag("KeyboardSimulatorModalDialogSubTable");

    private static int _vrInputActionIndex = 0;
    private static int _keyIndex = 0;
    private static bool _altLeft = false;
    private static bool _altRight = false;
    private static bool _ctrlLeft = false;
    private static bool _ctrlRight = false;
    private static bool _shiftLeft = false;
    private static bool _shiftRight = false;
    private static bool _metaLeft = false;
    private static bool _metaRight = false;

    private static void ParseEntry(string value)
    {
        var parts = value.Split("|");
        if (parts.Length != 3)
        {
            // Reset current values as we are likely registering a new entry.
            _vrInputActionIndex = 0;
            _keyIndex = 0;
            SetModifierFlags(ModifierFlags.None);
            return;
        }

        _vrInputActionIndex = Session.VrInputActionGuiIds.ToList().FindIndex(it => it.EndsWith($"##{parts[0]}"));
        _keyIndex = Session.KeyboardSimulatorKeyCodeGuiIds.ToList().FindIndex(it => it.EndsWith($"##{parts[1]}"));

        if (byte.TryParse(
                parts[2],
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out var modifierByte))
        {
            SetModifierFlags((ModifierFlags)modifierByte);
        }
        else
        {
            // To let NEW entries have zero flags instead of the previous ones.
            SetModifierFlags(ModifierFlags.None);
        }
    }

    private static string EncodeEntry()
    {
        var action = GuiUtils.GetTagFromId(Session.VrInputActionGuiIds[_vrInputActionIndex < 0 ? 0 : _vrInputActionIndex]);
        var key = GuiUtils.GetTagFromId(Session.KeyboardSimulatorKeyCodeGuiIds[_keyIndex < 0 ? 0 : _keyIndex]);
        var modifiers = GetModifierFlags();
        return $"{action}|{key}|{(byte)modifiers:X2}";
    }
    
    private static string DisplayEntry(string value)
    {
        var parts = value.Split("|");
        if (parts.Length != 3) return "N/A";

        Session.VrInputActionGuiTagToLabel.TryGetValue(parts[0], out var action);
        Session.KeyboardSimulatorKeyCodeGuiTagToLabel.TryGetValue(parts[1], out var key);
        byte.TryParse(parts[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var modifierByte);
        
        return $"{action} => [{DisplayFlags((ModifierFlags)modifierByte)}] {key}";
    }

    private static string RenderDialog(string startValue)
    {
        ImGui.Combo("VR Input Action", ref _vrInputActionIndex, Session.VrInputActionGuiIds, Session.VrInputActionGuiIds.Length);
        GuiUtils.DrawTooltip("The VR input action that will trigger the key simulation.");
        GuiUtils.DrawDivider();
        ImGui.Combo("Simulated Key", ref _keyIndex, Session.KeyboardSimulatorKeyCodeGuiIds, Session.KeyboardSimulatorKeyCodeGuiIds.Length);
        GuiUtils.DrawTooltip("Key that will be simulated on the selected VR input action.");

        ImGui.BeginTable(SubTableTag, 3);

        ImGui.TableNextColumn();
        GuiUtils.DrawRightAlignedText("Modifier", FontStyle.Bold);
        GuiUtils.DrawTooltip("Which modifier keys to hold down when simulating the key.");
        ImGui.TableNextColumn();
        GuiUtils.DrawText("Left", FontStyle.Bold);
        GuiUtils.DrawTooltip("Use the left side modifier key.");
        ImGui.TableNextColumn();
        GuiUtils.DrawText("Right", FontStyle.Bold);
        GuiUtils.DrawTooltip("Use the right side modifier key.");

        DrawRow("Alt", ref _altLeft, ref _altRight);
        DrawRow("Ctrl", ref _ctrlLeft, ref _ctrlRight);
        DrawRow("Shift", ref _shiftLeft, ref _shiftRight);
        DrawRow("OS", ref _metaLeft, ref _metaRight, "The Windows/Super/Option key, depending on platform.");

        ImGui.EndTable();

        return EncodeEntry();

        void DrawRow(string label, ref bool left, ref bool right, string tooltip = "")
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            GuiUtils.DrawRightAlignedText(label);
            GuiUtils.DrawTooltip(tooltip);
            ImGui.TableNextColumn();
            ImGui.Checkbox($"##{label}Left", ref left);
            ImGui.TableNextColumn();
            ImGui.Checkbox($"##{label}Right", ref right);
        }
    }

    private static void RenderList(string tag, string[] entries, ref int index, Action<string> add, Action<string> remove)
    {
        var sectionIndex = Settings.Current.Application.CurrentSection;
        var section = GuiStructure.Sections[sectionIndex];
        ImGui.PushStyleColor(ImGuiCol.ChildBg, section.AccentColor.Fade(0.375f));
        ImGui.BeginChild($"##keyboardSimulator{tag}Background", Vector2.Zero, ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.AlwaysUseWindowPadding);

        foreach (var universalEntry in entries)
        {
            GuiUtils.DoModal(
                $"Edit Entry##keyboardSimulator{tag}Edit{index}",
                "Edit",
                "Apply", "Cancel",
                universalEntry,
                () => ParseEntry(universalEntry),
                null,
                RenderDialog,
                value =>
                {
                    remove(universalEntry);
                    add(value);
                });

            ImGui.SameLine();
            ImGui.TextUnformatted(DisplayEntry(universalEntry));

            ImGui.SameLine();
            GuiUtils.DoModalToConfirm(
                $"Remove this entry?##keyboardSimulator{tag}Delete{index}",
                "Remove",
                "Yes",
                "No",
                () => { remove(universalEntry); }
            );
            index++;
        }

        ImGui.EndChild();
        ImGui.PopStyleColor();
    }

    [Flags]
    private enum ModifierFlags : byte
    {
        None = 0,

        AltLeft = 1 << 0,
        AltRight = 1 << 1,

        CtrlLeft = 1 << 2,
        CtrlRight = 1 << 3,

        ShiftLeft = 1 << 4,
        ShiftRight = 1 << 5,

        MetaLeft = 1 << 6,
        MetaRight = 1 << 7
    }

    private static ModifierFlags GetModifierFlags()
    {
        var flags = ModifierFlags.None;

        if (_altLeft) flags |= ModifierFlags.AltLeft;
        if (_altRight) flags |= ModifierFlags.AltRight;

        if (_ctrlLeft) flags |= ModifierFlags.CtrlLeft;
        if (_ctrlRight) flags |= ModifierFlags.CtrlRight;

        if (_shiftLeft) flags |= ModifierFlags.ShiftLeft;
        if (_shiftRight) flags |= ModifierFlags.ShiftRight;

        if (_metaLeft) flags |= ModifierFlags.MetaLeft;
        if (_metaRight) flags |= ModifierFlags.MetaRight;

        return flags;
    }

    private static void SetModifierFlags(ModifierFlags flags)
    {
        _altLeft = flags.HasFlag(ModifierFlags.AltLeft);
        _altRight = flags.HasFlag(ModifierFlags.AltRight);

        _ctrlLeft = flags.HasFlag(ModifierFlags.CtrlLeft);
        _ctrlRight = flags.HasFlag(ModifierFlags.CtrlRight);

        _shiftLeft = flags.HasFlag(ModifierFlags.ShiftLeft);
        _shiftRight = flags.HasFlag(ModifierFlags.ShiftRight);

        _metaLeft = flags.HasFlag(ModifierFlags.MetaLeft);
        _metaRight = flags.HasFlag(ModifierFlags.MetaRight);
    }

    private static string DisplayFlags(ModifierFlags flags)
    {
        var alt = flags.HasFlag(ModifierFlags.AltLeft) | flags.HasFlag(ModifierFlags.AltRight) ? "/" : "";
        var ctrl = flags.HasFlag(ModifierFlags.CtrlLeft) | flags.HasFlag(ModifierFlags.CtrlRight) ? "^" : "";
        var shift = flags.HasFlag(ModifierFlags.ShiftLeft) | flags.HasFlag(ModifierFlags.ShiftRight) ? "+" : "";
        var meta = flags.HasFlag(ModifierFlags.MetaLeft) | flags.HasFlag(ModifierFlags.MetaRight) ? "#" : "";
        return $"{alt}{ctrl}{shift}{meta}";
    }
}