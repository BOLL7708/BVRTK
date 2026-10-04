using System.Collections.Concurrent;
using BVRTK.Data;

namespace BVRTK.Components.KeyboardSimulator;

public class KeyboardSim
{
    private static ConcurrentDictionary<ulong, KeyboardSimulatorUtils.SimEntry> _generalEntries = [];
    private static ConcurrentDictionary<ulong, KeyboardSimulatorUtils.SimEntry> _gameEntries = [];

    private static void UnregisterAndRegister(string[] entries, ref ConcurrentDictionary<ulong, KeyboardSimulatorUtils.SimEntry> collection)
    {
        UnregisterEntries([.. collection.Keys]);
        RegisterEntries(entries, ref collection);
    }

    private static void RegisterEntries(string[] entries, ref ConcurrentDictionary<ulong, KeyboardSimulatorUtils.SimEntry> collection)
    {
        collection.Clear();
        foreach (var entryStr in entries)
        {
            var entry = KeyboardSimulatorUtils.ParseEntry(entryStr);
            var handle = RegisterEntry(entry);
            Console.WriteLine($"REGISTERED INPUT: {entry.Action.Name} -> {entry.KeyCode} handle: {handle} {long.MaxValue} {long.MinValue}");
            if (handle != ulong.MaxValue && handle != ulong.MinValue)
            {
                collection.TryAdd(handle, entry); // Handles are supposed to be unique so this should be safe.
            }
        }
    }

    private static ulong RegisterEntry(KeyboardSimulatorUtils.SimEntry entry)
    {
        var result = Services.Vr.Input.RegisterDigitalAction(
            entry.Action.Path,
            (data, info) =>
            {
                Console.WriteLine($"INPUT: active->{data.bActive} state->{data.bState} changed->{data.bChanged}");
                // TODO: Here we should in the future handle the different trigger modes too!
                // TODO: WE SHOULD ALSO SOMEHOW ADD THE MODIFIER KEYS! SHEESH! But test this first.
                if (data.bState) Services.SharpHook.SimulateKeyPress(entry.KeyCode);
            },
            entry.Action.IsChord
        );
        Console.WriteLine($"REGISTERED DIGITAL ACTION: {entry.Action.Path}");
        return result.ResultULong;
    }

    private static void UnregisterEntries(ulong[] handles)
    {
        Services.Vr.Input.UnregisterActions(handles);
    }

    /// <summary>
    /// Run this once to register all the callbacks for settings and for VR input listening.
    /// </summary>
    public static void Init()
    {
        // Register everything on first init if enabled
        if (Settings.Current.KeyboardSimulator.Enabled)
        {
            UnregisterAndRegister([.. Settings.Current.KeyboardSimulator.EntriesGeneral], ref _generalEntries);
            Settings.Current.KeyboardSimulator.EntriesPerGame.TryGetValue(Session.SteamSceneAppId, out var gameEntries);
            UnregisterAndRegister(gameEntries ?? [], ref _gameEntries);
        }

        // React to enabling/disabling
        SettingsChangeHandlers.OnKeyboardSimulatorEnabledChanged += on =>
        {
            UnregisterEntries([.. _generalEntries.Keys]);
            UnregisterEntries([.. _gameEntries.Keys]);
            if (!on) return;

            RegisterEntries([.. Settings.Current.KeyboardSimulator.EntriesGeneral], ref _generalEntries);
            Settings.Current.KeyboardSimulator.EntriesPerGame.TryGetValue(Session.SteamSceneAppId, out var gameEntries);
            RegisterEntries(gameEntries ?? [], ref _gameEntries);
        };

        // React to game changing
        Session.OnSteamSceneAppIdChanged += appId =>
        {
            if (!Settings.Current.KeyboardSimulator.Enabled) return;
            
            UnregisterEntries([.. _gameEntries.Keys]);
            Settings.Current.KeyboardSimulator.EntriesPerGame.TryGetValue(appId, out var gameEntries);
            RegisterEntries(gameEntries ?? [], ref _gameEntries);
        };

        // React to general entries changing
        SettingsChangeHandlers.OnKeyboardSimulatorEntriesGeneralChanged += list =>
        {
            if (!Settings.Current.KeyboardSimulator.Enabled) return;

            UnregisterEntries([.. _generalEntries.Keys]);
            _generalEntries.Clear();
            RegisterEntries([.. list], ref _generalEntries);
        };

        // React to game specific entries changing
        SettingsChangeHandlers.OnKeyboardSimulatorEntriesPerGameChanged += list =>
        {
            if (!Settings.Current.KeyboardSimulator.Enabled) return;

            UnregisterEntries([.. _gameEntries.Keys]);
            list.TryGetValue(Session.SteamSceneAppId, out var gameEntries);
            RegisterEntries(gameEntries ?? [], ref _gameEntries);
        };
    }
}