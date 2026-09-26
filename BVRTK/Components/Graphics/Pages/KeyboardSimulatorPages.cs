using System.Globalization;
using System.Numerics;
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
            value => Settings.Current.KeyboardSimulator.AddIfNewToEntriesGeneral(value)
        );

        var generalIndex = 0;
        RenderList(
            "general",
            [.. Settings.Current.KeyboardSimulator.EntriesGeneral],
            ref generalIndex,
            Settings.Current.KeyboardSimulator.ReplaceInEntriesGeneral,
            Settings.Current.KeyboardSimulator.MoveUpInEntriesGeneral,
            Settings.Current.KeyboardSimulator.MoveDownInEntriesGeneral
        );

        if (Session.SteamSceneAppId.Length > 0)
        {
            GuiUtils.DrawTitle($"Entries for: {Session.SteamSceneAppName}");

            GuiUtils.DoModal<string>(
                "Add Game Specific Keyboard Simulation Entry",
                "Add game specific entry",
                "Add", "Cancel",
                "", // Empty as we start from nothing
                () =>
                {
                    ParseEntry(""); // Will reset the entry values
                },
                (value) =>
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted("Will be used when a specific game is running:");
                    // No additional GUI needed as we are just displaying the add button.
                },
                RenderDialog,
                AddGameEntry
            );

            var gameIndex = 0;
            Settings.Current.KeyboardSimulator.EntriesPerGame.TryGetValue(Session.SteamSceneAppId, out var entries);
            if (entries != null)
                RenderList(
                    "game",
                    entries,
                    ref gameIndex,
                    ReplaceGameEntry,
                    MoveGameEntryUp,
                    MoveGameEntryDown
                );
        }
        else
        {
            GuiUtils.DrawTitle("Game Specific Entries");
            GuiUtils.DrawCenteredText("No game detected.");
        }
    }


    private static void AddGameEntry(string entry)
    {
        Settings.Current.KeyboardSimulator.EntriesPerGame.TryGetValue(Session.SteamSceneAppId, out var existingItems);
        if (existingItems != null && existingItems.Contains(entry)) return;
        var list = existingItems ?? [];
        Settings.Current.KeyboardSimulator.SetInEntriesPerGame(Session.SteamSceneAppId, [.. list, entry]);
    }

    private static void ReplaceGameEntry(string? currentEntry, string? newEntry)
    {
        Settings.Current.KeyboardSimulator.EntriesPerGame.TryGetValue(Session.SteamSceneAppId, out var existingItems);
        var list = existingItems?.ToList() ?? [];

        if (currentEntry == null && newEntry == null) return;
        if (currentEntry != null && newEntry != null)
        {
            var index = list.IndexOf(currentEntry);
            if (index >= 0)
            {
                list[index] = newEntry;
            }
        }

        if (currentEntry == null && newEntry != null) list.Add(newEntry);
        if (currentEntry != null && newEntry == null) list.Remove(currentEntry);

        Settings.Current.KeyboardSimulator.SetInEntriesPerGame(Session.SteamSceneAppId, [.. list]);
    }

    private static void MoveGameEntryUp(int index)
    {
        if (index < 0) return;

        Settings.Current.KeyboardSimulator.EntriesPerGame.TryGetValue(Session.SteamSceneAppId, out var existingItems);
        var list = existingItems?.ToList() ?? [];
        var item = list[index];
        list.RemoveAt(index);
        list.Insert(index - 1, item);
        
        Settings.Current.KeyboardSimulator.SetInEntriesPerGame(Session.SteamSceneAppId, [.. list]);
    }

    private static void MoveGameEntryDown(int index)
    {
        Settings.Current.KeyboardSimulator.EntriesPerGame.TryGetValue(Session.SteamSceneAppId, out var existingItems);
        var list = existingItems?.ToList() ?? [];

        if (index >= list.Count - 1) return;

        var item = list[index];
        list.RemoveAt(index);
        list.Insert(index + 1, item);
        
        Settings.Current.KeyboardSimulator.SetInEntriesPerGame(Session.SteamSceneAppId, [.. list]);
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
    private static string _label = "";

    private static void ParseEntry(string value)
    {
        var parts = value.Split("|", 4);
        if (parts.Length < 3)
        {
            // Reset current values as we are likely registering a new entry.
            _vrInputActionIndex = 0;
            _keyIndex = 0;
            _label = "";
            SetModifierFlags(ModifierFlags.None);
            return;
        }

        _vrInputActionIndex = Session.VrInputActionGuiIds.ToList().FindIndex(it => it.EndsWith($"##{parts[0]}"));
        _keyIndex = Session.KeyboardSimulatorKeyCodeGuiIds.ToList().FindIndex(it => it.EndsWith($"##{parts[1]}"));
        _label = parts.Length >= 4 ? parts[3] : "";

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
        return $"{action}|{key}|{(byte)modifiers:X2}|{_label}";
    }

    private static string[] DisplayEntry(string value)
    {
        var parts = value.Split("|", 4);
        if (parts.Length < 3) return ["N/A"];

        Session.VrInputActionGuiTagToLabel.TryGetValue(parts[0], out var action);
        Session.KeyboardSimulatorKeyCodeGuiTagToLabel.TryGetValue(parts[1], out var key);
        byte.TryParse(parts[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var modifierByte);
        var label = parts.Length >= 4 ? parts[3] : "";
        var description = $"{action} => {DisplayFlags((ModifierFlags)modifierByte)}{key}";
        return label.Length > 0 ? [label, description] : [description];
    }

    private static string RenderDialog(string startValue)
    {
        ImGui.Combo("VR Input Action", ref _vrInputActionIndex, Session.VrInputActionGuiIds, Session.VrInputActionGuiIds.Length);
        GuiUtils.DrawTooltip("The VR input action that will trigger the key simulation.");
        GuiUtils.DrawDivider();
        ImGui.Combo("Simulated Key", ref _keyIndex, Session.KeyboardSimulatorKeyCodeGuiIds, Session.KeyboardSimulatorKeyCodeGuiIds.Length);
        GuiUtils.DrawTooltip("Key that will be simulated on the selected VR input action.");

        ImGui.BeginTable(SubTableTag, 5);
        ImGui.TableNextColumn();
        GuiUtils.DrawRightAlignedText("Modifier", FontStyle.Bold);
        GuiUtils.DrawTooltip("Which modifier keys to hold down when simulating the key.");
        ImGui.TableNextColumn();
        GuiUtils.DrawText("Alt", FontStyle.Bold);
        ImGui.TableNextColumn();
        GuiUtils.DrawText("Ctrl", FontStyle.Bold);
        ImGui.TableNextColumn();
        GuiUtils.DrawText("Shift", FontStyle.Bold);
        ImGui.TableNextColumn();
        GuiUtils.DrawText("OS", FontStyle.Bold);
        GuiUtils.DrawTooltip("The Windows/Super/Option key, depending on platform.");
        DrawRow("Left", ref _altLeft, ref _ctrlLeft, ref _shiftLeft, ref _metaLeft, "Use the left side modifier key(s).");
        DrawRow("Right", ref _altRight, ref _ctrlRight, ref _shiftRight, ref _metaRight, "Use the right side modifier key(s).");
        ImGui.EndTable();

        ImGui.InputText("Optional name", ref _label, 32, ImGuiInputTextFlags.None);

        return EncodeEntry();

        void DrawRow(string label, ref bool alt, ref bool ctrl, ref bool shift, ref bool meta, string tooltip = "")
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            GuiUtils.DrawRightAlignedText(label);
            GuiUtils.DrawTooltip(tooltip);
            ImGui.TableNextColumn();
            ImGui.Checkbox($"##{label}Alt", ref alt);
            ImGui.TableNextColumn();
            ImGui.Checkbox($"##{label}Ctrl", ref ctrl);
            ImGui.TableNextColumn();
            ImGui.Checkbox($"##{label}Shift", ref shift);
            ImGui.TableNextColumn();
            ImGui.Checkbox($"##{label}OS", ref meta);
        }
    }

    private static int _listDraggedIndex = -1;

    private static void RenderList(string tag, string[] entries, ref int index, Action<string?, string?> replace, Action<int> moveUp, Action<int> moveDown)
    {
        var sectionIndex = Settings.Current.Application.CurrentSection;
        var section = GuiStructure.Sections[sectionIndex];
        ImGui.PushStyleColor(ImGuiCol.ChildBg, section.AccentColor.Fade(0.30f));
        ImGui.PushStyleColor(ImGuiCol.TableRowBg, section.AccentColor.Fade(0.35f));
        ImGui.PushStyleColor(ImGuiCol.TableRowBgAlt, section.AccentColor.Fade(0.25f));
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, Constants.GuiItemSpacing);
        ImGui.BeginChild($"##keyboardSimulator{tag}Child", Vector2.Zero, ImGuiChildFlags.AlwaysUseWindowPadding | ImGuiChildFlags.AutoResizeY);

        if (ImGui.BeginTable($"##keyboardSimulator{tag}Background", 3, ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn($"##keyboardSimulator{tag}LeftCol", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn($"##keyboardSimulator{tag}MiddleCol", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn($"##keyboardSimulator{tag}RightCol", ImGuiTableColumnFlags.WidthFixed);
            foreach (var universalEntry in entries)
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Dummy(Vector2.Zero);
                ImGui.SameLine();
                GuiUtils.DoModal(
                    $"Edit Entry##keyboardSimulator{tag}Edit{index}",
                    "Edit",
                    "Apply", "Cancel",
                    universalEntry,
                    () => ParseEntry(universalEntry),
                    null,
                    RenderDialog,
                    value => { replace(universalEntry, value); });

                ImGui.TableNextColumn();
                var description = DisplayEntry(universalEntry);

                ImGui.Text(description[0]);
                if (description.Length == 2) GuiUtils.DrawTooltip(description[1]);

                ImGui.TableNextColumn();
                ImGui.BeginDisabled(index == 0);
                if (ImGui.ArrowButton($"##keyboardSimulator{tag}MoveUp{index}", ImGuiDir.Up)) moveUp(index);
                ImGui.EndDisabled();
                ImGui.SameLine();
                ImGui.BeginDisabled(index == entries.Length - 1);
                if (ImGui.ArrowButton($"##keyboardSimulator{tag}MoveDown{index}", ImGuiDir.Down)) moveDown(index);
                ImGui.EndDisabled();
                ImGui.SameLine();
                ImGui.Dummy(Vector2.Zero);
                ImGui.SameLine();
                GuiUtils.DoModalToConfirm(
                    $"Remove this entry?##keyboardSimulator{tag}Delete{index}",
                    "X",
                    string.Join("\n", description),
                    "Yes",
                    "No",
                    () => { replace(universalEntry, null); }
                );
                ImGui.SameLine();
                ImGui.Dummy(Vector2.Zero);

                index++;
            }

            ImGui.EndTable();
        }

        ImGui.EndChild();
        ImGui.PopStyleVar();
        ImGui.PopStyleColor(3);
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
        var all = $"{alt}{ctrl}{shift}{meta}";
        return all.IsWhiteSpace() ? "" : $"[{all}] ";
    }
}