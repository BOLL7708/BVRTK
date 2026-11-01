using System.Numerics;
using BVRTK.Components.KeyboardSimulator;
using BVRTK.Data;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics.Pages;

public static class KeyboardSimulatorPages
{
    #region Pages

    public static void ManualEntries()
    {
        GuiUtils.DoModal<string>(
            "Add Manual Group",
            "Add group",
            "Add", "Cancel",
            "", // Empty as we start from nothing
            () => { _manualGroupName = ""; },
            _ =>
            {
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted("Groups of entries that can be toggled manually:");
                // No additional GUI needed as we are just displaying the add button.
            },
            _ =>
            {
                ImGui.InputText("Group name", ref _manualGroupName, 32, ImGuiInputTextFlags.None);
                return _manualGroupName;
            },
            value =>
            {
                if (value.Trim().Length == 0) return;
                var count = Settings.Current.KeyboardSimulator.EntriesManual.Count;
                var group = new KeyboardSimulatorUtils.SimGroup(0, true, value.Trim());
                var key = KeyboardSimulatorUtils.EncodeGroup(group, count + 1);
                Settings.Current.KeyboardSimulator.SetInEntriesManual(key, []);
                Settings.Current.KeyboardSimulator.SetEntriesManual(Settings.Current.KeyboardSimulator.EntriesManual.ReserializeKeys());
            });

        var manualCount = Settings.Current.KeyboardSimulator.EntriesManual.Count;
        foreach (var entry in Settings.Current.KeyboardSimulator.EntriesManual)
        {
            ImGui.PushID(entry.Key);
            ImGui.Dummy(Constants.Gui.ItemSpacing);

            RenderList(
                "manual",
                entry.Key,
                entry.Value,
                (currentValue, newValue) => ReplaceManualEntry(entry.Key, currentValue, newValue),
                (index, move) => MoveManualEntry(entry.Key, index, move),
                entryStr => AddManualEntry(entry.Key, entryStr),
                newKey =>
                {
                    Settings.Current.KeyboardSimulator.SetEntriesManual(
                        Settings.Current.KeyboardSimulator.EntriesManual.RenameKey(entry.Key, newKey)
                    );
                },
                () =>
                {
                    Settings.Current.KeyboardSimulator.SetEntriesManual(
                        Settings.Current.KeyboardSimulator.EntriesManual.RemoveKey(entry.Key)
                    );
                },
                move =>
                {
                    Settings.Current.KeyboardSimulator.SetEntriesManual(
                        Settings.Current.KeyboardSimulator.EntriesManual.RearrangeKey(entry.Key, move)
                    );
                },
                null,
                true,
                manualCount
            );
            ImGui.PopID();
        }
    }

    public static void GameEntries()
    {
        if (Session.SteamSceneAppId.Length > 0)
        {
            ImGui.PushID(Session.SteamSceneAppId);
            
            ImGui.TextUnformatted("Groups of entries that gets activated by the running VR game.");
            ImGui.Dummy(Constants.Gui.ItemSpacing);

            var entries = GetEntriesForCurrentGame(); // Also updates _previouslyMatchedGameKey
            RenderList(
                "game",
                _previouslyMatchedGameKey,
                entries,
                ReplaceGameEntry,
                MoveGameEntry,
                AddGameEntry,
                value =>
                {
                    Settings.Current.KeyboardSimulator.SetEntriesPerGame(
                        Utils.RenameKeyInDictionary<IDStrArr, string, string[]>(Settings.Current.KeyboardSimulator.EntriesPerGame, _previouslyMatchedGameKey, value)
                    );
                    _previouslyMatchedGameKey = value; // This should always be the current game as that is the only one we can rename by toggling enabled status.
                },
                null,
                null,
                "VR Game: " + Session.SteamSceneAppName
            );

            ImGui.PopID();
        }
        else
        {
            GuiUtils.DrawCenteredText("No game detected.");
        }
    }

    public static void Monitor()
    {
        GuiUtils.DrawText("Active Key Entries", FontStyle.Bold);
        foreach(var (_, entry) in KeyboardSim.ManualEntries)
        {
            ImGui.TextUnformatted($"{entry.Action.Name} -> {entry.KeyCode}");
        }
        foreach(var (_, entry) in KeyboardSim.GameEntries)
        {
            ImGui.TextUnformatted($"{entry.Action.Name} -> {entry.KeyCode}");
        }
    }

    public static void Help()
    {
    }

    #endregion

    #region Manual Entries

    private static void AddManualEntry(string group, string entry)
    {
        Settings.Current.KeyboardSimulator.SetEntriesManual(Utils.AddToDictionaryArray(
            Settings.Current.KeyboardSimulator.EntriesManual, group, entry
        ));
    }

    private static void ReplaceManualEntry(string group, string? currentEntry, string? newEntry)
    {
        if (currentEntry == null && newEntry == null) return;
        if (currentEntry != null && newEntry != null)
        {
            Settings.Current.KeyboardSimulator.SetEntriesManual(Utils.ReplaceInDictionaryArray(
                Settings.Current.KeyboardSimulator.EntriesManual, group, currentEntry, newEntry
            ));
        }

        if (currentEntry == null && newEntry != null)
        {
            Settings.Current.KeyboardSimulator.SetEntriesManual(Utils.AddToDictionaryArray(
                Settings.Current.KeyboardSimulator.EntriesManual, group, newEntry
            ));
        }

        if (currentEntry != null && newEntry == null)
        {
            Settings.Current.KeyboardSimulator.SetEntriesManual(Utils.RemoveFromDictionaryArray(
                Settings.Current.KeyboardSimulator.EntriesManual, group, currentEntry
            ));
        }
    }

    private static void MoveManualEntry(string group, int index, int move)
    {
        Settings.Current.KeyboardSimulator.SetEntriesManual(Utils.RearrangeDictionaryArray<ISDStrArr, string, string>(
            Settings.Current.KeyboardSimulator.EntriesManual, group, index, index + move
        ));
    }

    #endregion

    #region Game Entries

    private static string _previousSteamAppId = "";
    private static string _previouslyMatchedGameKey = "";

    private static string[] GetEntriesForCurrentGame()
    {
        if (Session.SteamSceneAppId != _previousSteamAppId)
        {
            _previousSteamAppId = Session.SteamSceneAppId;
            _previouslyMatchedGameKey = "";
            var match = Settings.Current.KeyboardSimulator.EntriesPerGame.ToList().Find(kvp =>
            {
                var group = KeyboardSimulatorUtils.ParseGroup(kvp.Key);
                return group.Name == Session.SteamSceneAppId;
            });
            if (match.Key is { Length: > 0 }) // Match here is not null but the Key and Value will be if there is no 
            {
                _previouslyMatchedGameKey = match.Key;
            }
        }

        if (_previouslyMatchedGameKey.Length == 0) return [];

        Settings.Current.KeyboardSimulator.EntriesPerGame.TryGetValue(_previouslyMatchedGameKey, out var entries);
        return entries ?? [];
    }

    private static void AddGameEntry(string entry)
    {
        Settings.Current.KeyboardSimulator.SetEntriesPerGame(Utils.AddToDictionaryArray(
            Settings.Current.KeyboardSimulator.EntriesPerGame, _previouslyMatchedGameKey, entry
        ));
    }

    private static void ReplaceGameEntry(string? currentEntry, string? newEntry)
    {
        if (currentEntry == null && newEntry == null) return;
        if (currentEntry != null && newEntry != null)
        {
            Settings.Current.KeyboardSimulator.SetEntriesPerGame(Utils.ReplaceInDictionaryArray(
                Settings.Current.KeyboardSimulator.EntriesPerGame, _previouslyMatchedGameKey, currentEntry, newEntry
            ));
        }

        if (currentEntry == null && newEntry != null)
        {
            Settings.Current.KeyboardSimulator.SetEntriesPerGame(Utils.AddToDictionaryArray(
                Settings.Current.KeyboardSimulator.EntriesPerGame, _previouslyMatchedGameKey, newEntry
            ));
        }

        if (currentEntry != null && newEntry == null)
        {
            var newEntries = Utils.RemoveFromDictionaryArray(
                Settings.Current.KeyboardSimulator.EntriesPerGame, _previouslyMatchedGameKey, currentEntry
            );
            newEntries.TryGetValue(_previouslyMatchedGameKey, out var newArray);
            Settings.Current.KeyboardSimulator.SetEntriesPerGame(
                (newArray ?? []).Length == 0 
                    ? newEntries.Remove(_previouslyMatchedGameKey) // We clear an empty group from the list to not end up with a bunch of empty ones.
                    : newEntries
            );
        }
    }

    private static void MoveGameEntry(int index, int move)
    {
        Settings.Current.KeyboardSimulator.SetEntriesPerGame(Utils.RearrangeDictionaryArray<IDStrArr, string, string>(
            Settings.Current.KeyboardSimulator.EntriesPerGame, _previouslyMatchedGameKey, index, index + move
        ));
    }

    #endregion

    private static readonly string ModifierTableTag = GuiUtils.GetNextSerialTag("KeyboardSimulatorModalDialogModifierTable");
    private static readonly string ModifierAndTriggerColumnsTag = GuiUtils.GetNextSerialTag("KeyboardSimulatorModalDialogModifierAndTriggerColumns");

    #region Group

    private static string _manualGroupName = "";

    #endregion

    #region Entry

    private static int _vrInputActionIndex;
    private static int _keyIndex;
    private static bool _altLeft;
    private static bool _altRight;
    private static bool _ctrlLeft;
    private static bool _ctrlRight;
    private static bool _shiftLeft;
    private static bool _shiftRight;
    private static bool _metaLeft;
    private static bool _metaRight;
    private static int _triggerIndex;
    private static int _triggerInterval = 100;
    private static string _label = "";

    private static void ParseEntry(string value)
    {
        var entry = KeyboardSimulatorUtils.ParseEntry(value);
        _vrInputActionIndex = entry.ActionIndex;
        _keyIndex = entry.KeyCodeIndex;
        SetModifierFlags(entry.Modifiers);
        _triggerIndex = entry.TriggerIndex;
        _triggerInterval = entry.TriggerInterval;
        _label = entry.Label;
    }

    private static string EncodeEntry()
    {
        var entry = new KeyboardSimulatorUtils.SimEntry
        {
            ActionIndex = _vrInputActionIndex,
            KeyCodeIndex = _keyIndex,
            Modifiers = GetModifierFlags(),
            TriggerIndex = _triggerIndex,
            TriggerInterval = _triggerInterval,
            Label = _label
        };
        return KeyboardSimulatorUtils.EncodeEntry(entry);
    }

    private static string[] DisplayEntry(string value)
    {
        var entry = KeyboardSimulatorUtils.ParseEntry(value);
        var actionPrompt = entry.Action.Prompt();
        Session.KeyboardSimulatorKeyCodeGuiTagToLabel.TryGetValue(entry.KeyCode.ToString(), out var keyCodeName);
        var description = $"{actionPrompt} => {DisplayFlags(entry.Modifiers)}{keyCodeName}";
        return entry.Label.Length > 0 ? [entry.Label, description] : [description];
    }

    #endregion

    private static string RenderDialog(string startValue)
    {
        Session.ActionGuiIds.TryGetValue(Constants.ActionSet.KeyboardSim, out var actionGuiIds);
        ImGui.Combo("VR Input Action", ref _vrInputActionIndex, actionGuiIds ?? [], actionGuiIds?.Length ?? 0);
        GuiUtils.DrawTooltip("The VR input action that will trigger the key simulation.");
        GuiUtils.DrawDivider();
        ImGui.Combo("Simulated Key", ref _keyIndex, Session.KeyboardSimulatorKeyCodeGuiIds, Session.KeyboardSimulatorKeyCodeGuiIds.Length);
        GuiUtils.DrawTooltip("Key that will be simulated on the selected VR input action.");

        ImGui.Columns(2, ModifierAndTriggerColumnsTag, false);
        ImGui.SetColumnWidth(0, Constants.Overlay.GuiScale * 230f);

        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, _defaultCellPadding); // Resetting padding here as otherwise the edit modal will inherit it from the entry list.
        if (ImGui.BeginTable(ModifierTableTag, 5, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableNextColumn();
            GuiUtils.DrawText("Modifier", FontStyle.Bold);
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
        }

        ImGui.PopStyleVar();

        ImGui.NextColumn();

        ImGui.TextUnformatted("When to trigger");

        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("On");
        ImGui.SameLine();
        ImGui.Combo("##KeyboardSim.Trigger.When", ref _triggerIndex, Session.KeyboardSimulatorTriggerGuiIds, Session.KeyboardSimulatorTriggerGuiIds.Length);

        var isRepeatOn = Enum.GetValues<HardwareInputTrigger>()[_triggerIndex] == HardwareInputTrigger.Repeat;
        var intervalFlags = isRepeatOn ? ImGuiInputTextFlags.None : ImGuiInputTextFlags.ReadOnly;
        ImGui.InputInt("Interval", ref _triggerInterval, 10, 100, intervalFlags);
        _triggerInterval = Math.Clamp(_triggerInterval, 1, 99999);
        if (isRepeatOn) GuiUtils.DrawTooltip("The number of milliseconds between each repeated key simulation.");

        ImGui.Columns(1);

        GuiUtils.DrawDivider();
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

    private static Vector2 _defaultCellPadding = Vector2.Zero;

    private static void RenderList(
        string tag,
        string entryKey,
        string[] entries,
        Action<string?, string?> replace,
        Action<int, int> move,
        Action<string> addEntry,
        Action<string> renameGroup,
        Action? deleteGroup = null,
        Action<int>? moveGroup = null,
        string? titleOverride = null,
        bool allowGroupEdit = false,
        int totalGroups = 0
    )
    {
        if (_defaultCellPadding == Vector2.Zero) _defaultCellPadding = ImGui.GetStyle().CellPadding;
        var sectionIndex = Settings.Current.Application.CurrentSection;
        var section = GuiStructure.Sections[sectionIndex];
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, Constants.Gui.ItemSpacing);
        ImGui.PushID(tag);
        ImGui.BeginChild("##keyboardSimulatorChild", Vector2.Zero, ImGuiChildFlags.None | ImGuiChildFlags.AutoResizeY);

        #region Group Title

        ImGui.PushStyleColor(ImGuiCol.TableRowBg, section.AccentColor.Fade(0.40f));

        if (ImGui.BeginTable("##keyboardSimulatorGroupTable", 3, ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("##keyboardSimulatorGroupLeftCol", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("##keyboardSimulatorGroupMiddleCol", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("##keyboardSimulatorGroupRightCol", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            ImGui.Dummy(Vector2.Zero);
            ImGui.SameLine();

            // ALWAYS Checkbox for enabled
            var group = KeyboardSimulatorUtils.ParseGroup(entryKey);
            ImGui.PushID(entryKey);
            var enabled = group.Enabled;
            if (ImGui.Checkbox("##ManualEnabled", ref enabled))
            {
                group.Enabled = enabled;
                renameGroup.Invoke(KeyboardSimulatorUtils.EncodeGroup(group, totalGroups));
                SkipFrames();
            }

            GuiUtils.DrawTooltip(enabled ? "Uncheck to disable" : "Check to enable.");

            ImGui.SameLine();

            // ALWAYS Add entry
            GuiUtils.DoModal<string>(
                "Add new Keyboard Simulation Entry",
                "Add entry",
                "Add", "Cancel",
                "", // Empty as we start from nothing
                () =>
                {
                    ParseEntry(""); // Will reset the entry values
                },
                null,
                RenderDialog,
                value =>
                {
                    addEntry(value);
                    SkipFrames();
                });

            ImGui.TableNextColumn();

            GuiUtils.DrawText(
                titleOverride ?? (group.Name.Length > 0
                    ? group.Name
                    : "N/A"),
                FontStyle.Bold
            );

            ImGui.TableNextColumn();

            if (allowGroupEdit)
            {
                ImGui.PushID(entryKey);

                ImGui.SameLine();
                // EDIT Rename
                GuiUtils.DoModal<string>(
                    "Rename Manual Group##ManualGroupRename",
                    "Rename",
                    "Rename", "Cancel",
                    group.Name,
                    () => { _manualGroupName = group.Name; },
                    null,
                    _ =>
                    {
                        ImGui.InputText("Group name", ref _manualGroupName, 32, ImGuiInputTextFlags.None);
                        return _manualGroupName;
                    },
                    value =>
                    {
                        group.Name = value;
                        renameGroup.Invoke(KeyboardSimulatorUtils.EncodeGroup(group, totalGroups));
                        SkipFrames();
                    });

                ImGui.SameLine();

                // EDIT Reorder-arrows
                ImGui.BeginDisabled(group.Order == 1);
                if (ImGui.ArrowButton("##ManualGroupMoveUp", ImGuiDir.Up))
                {
                    moveGroup?.Invoke(-1);
                    SkipFrames();
                }

                ImGui.EndDisabled();
                ImGui.SameLine();
                ImGui.BeginDisabled(group.Order == totalGroups);
                if (ImGui.ArrowButton("##ManualGroupMoveDown", ImGuiDir.Down))
                {
                    moveGroup?.Invoke(1);
                    SkipFrames();
                }

                ImGui.EndDisabled();

                ImGui.SameLine();

                // EDIT Delete X
                ImGui.Dummy(Vector2.Zero);
                ImGui.SameLine();

                GuiUtils.DoModalToConfirm(
                    "Remove this entry?##ManualGroupDelete",
                    "X",
                    "Do you want to delete this whole group?",
                    "Yes",
                    "No",
                    () => deleteGroup?.Invoke()
                );

                ImGui.PopID();
            }

            ImGui.SameLine();
            ImGui.Dummy(Vector2.Zero);
            ImGui.PopID();
            ImGui.EndTable();
        }

        ImGui.PopStyleColor();

        #endregion

        // Make the tables touch, instead of toggling item spacing off and on again all over the place, we do this offset.
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGui.GetStyle().ItemSpacing.Y);

        #region Entry Rows

        ImGui.PushStyleColor(ImGuiCol.TableRowBg, section.AccentColor.Fade(0.20f));
        ImGui.PushStyleColor(ImGuiCol.TableRowBgAlt, section.AccentColor.Fade(0.30f));

        if (ImGui.BeginTable("##keyboardSimulatorEntriesTable", 3, ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("##keyboardSimulatorEntryLeftCol", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("##keyboardSimulatorEntryMiddleCol", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("##keyboardSimulatorEntryRightCol", ImGuiTableColumnFlags.WidthFixed);

            if (entries.Length == 0)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Dummy(Vector2.Zero);
                ImGui.SameLine();
                ImGui.TextUnformatted("No entries.");
            }
            else
            {
                var index = 0;
                foreach (var universalEntry in entries)
                {
                    ImGui.PushID(index);
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Dummy(Vector2.Zero);
                    ImGui.SameLine();

                    // Edit button
                    GuiUtils.DoModal(
                        "Edit Entry##keyboardSimulatorEdit",
                        "Edit",
                        "Apply", "Cancel",
                        universalEntry,
                        () => ParseEntry(universalEntry),
                        null,
                        RenderDialog,
                        value => { replace(universalEntry, value); });

                    ImGui.TableNextColumn();

                    // Text with conditional tooltip
                    var description = DisplayEntry(universalEntry);
                    ImGui.Text(description[0]);
                    if (description.Length == 2) GuiUtils.DrawTooltip(description[1]);

                    ImGui.TableNextColumn();

                    // Up & down buttons
                    ImGui.BeginDisabled(index == 0);
                    if (ImGui.ArrowButton("##keyboardSimulatorMoveUp", ImGuiDir.Up)) move(index, -1);
                    ImGui.EndDisabled();
                    ImGui.SameLine();
                    ImGui.BeginDisabled(index == entries.Length - 1);
                    if (ImGui.ArrowButton("##keyboardSimulatorMoveDown", ImGuiDir.Down)) move(index, 1);
                    ImGui.EndDisabled();

                    ImGui.SameLine();
                    ImGui.Dummy(Vector2.Zero);
                    ImGui.SameLine();

                    // Delete button
                    GuiUtils.DoModalToConfirm(
                        "Remove this entry?##keyboardSimulatorDelete",
                        "X",
                        string.Join("\n", description),
                        "Yes",
                        "No",
                        () => { replace(universalEntry, null); }
                    );

                    ImGui.SameLine();
                    ImGui.Dummy(Vector2.Zero);
                    ImGui.PopID();
                    index++;
                }
            }

            ImGui.EndTable();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(2);
        ImGui.EndChild();
        ImGui.PopID();

        #endregion
    }


    private static KeyboardSimulatorUtils.ModifierFlags GetModifierFlags()
    {
        var flags = KeyboardSimulatorUtils.ModifierFlags.None;

        if (_altLeft) flags |= KeyboardSimulatorUtils.ModifierFlags.AltLeft;
        if (_altRight) flags |= KeyboardSimulatorUtils.ModifierFlags.AltRight;

        if (_ctrlLeft) flags |= KeyboardSimulatorUtils.ModifierFlags.CtrlLeft;
        if (_ctrlRight) flags |= KeyboardSimulatorUtils.ModifierFlags.CtrlRight;

        if (_shiftLeft) flags |= KeyboardSimulatorUtils.ModifierFlags.ShiftLeft;
        if (_shiftRight) flags |= KeyboardSimulatorUtils.ModifierFlags.ShiftRight;

        if (_metaLeft) flags |= KeyboardSimulatorUtils.ModifierFlags.MetaLeft;
        if (_metaRight) flags |= KeyboardSimulatorUtils.ModifierFlags.MetaRight;

        return flags;
    }

    private static void SetModifierFlags(KeyboardSimulatorUtils.ModifierFlags flags)
    {
        _altLeft = flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.AltLeft);
        _altRight = flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.AltRight);

        _ctrlLeft = flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.CtrlLeft);
        _ctrlRight = flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.CtrlRight);

        _shiftLeft = flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.ShiftLeft);
        _shiftRight = flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.ShiftRight);

        _metaLeft = flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.MetaLeft);
        _metaRight = flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.MetaRight);
    }

    private static string DisplayFlags(KeyboardSimulatorUtils.ModifierFlags flags)
    {
        var alt = flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.AltLeft) | flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.AltRight) ? "/" : "";
        var ctrl = flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.CtrlLeft) | flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.CtrlRight) ? "^" : "";
        var shift = flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.ShiftLeft) | flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.ShiftRight) ? "+" : "";
        var meta = flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.MetaLeft) | flags.HasFlag(KeyboardSimulatorUtils.ModifierFlags.MetaRight) ? "#" : "";
        var all = $"{alt}{ctrl}{shift}{meta}";
        return all.IsWhiteSpace() ? "" : $"[{all}] ";
    }

    private static void SkipFrames()
    {
        Session.GuiFlags.SkipFrames = Constants.Gui.SkipFramesInterface;
    }
}