using System.Collections.Concurrent;
using System.Diagnostics;
using BVRTK.Data;
using SharpHook.Data;
using SharpHook.Simulation;

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

    private static void SimKey(KeyCode keyCode, bool? down = null)
    {
        if (keyCode == KeyCode.VcUndefined) return;

        var result = down switch
        {
            true => Services.SharpHook.SimulateKeyPress(keyCode),
            false => Services.SharpHook.SimulateKeyRelease(keyCode),
            _ => Services.SharpHook.SimulateKeyStroke(keyCode)
        };
        if (result != UioHookResult.Success)
        {
            // TODO: Hook this up to some proper logging or even something that sends a notification into VR.
            Console.WriteLine($"ERROR SIMULATING KEY: {result}");
        }
    }

    private static void ApplyModifiers(KeyCode[] modifiers, bool down)
    {
        foreach (var modifier in modifiers)
        {
            SimKey(modifier, down);
        }
    }

    private static readonly ConcurrentDictionary<string, RepeatRunner> Repeaters = new();


    private sealed class RepeatRunner(KeyCode keyCode, KeyCode[] modifiers, int intervalMs) : IDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private Task? _task;

        public void Start() => _task = Task.Run(Loop);

        private void Loop()
        {
            ApplyModifiers(modifiers, true);
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var next = TimeSpan.Zero;
                var interval = TimeSpan.FromMilliseconds(intervalMs);

                while (!_cts.IsCancellationRequested)
                {
                    SimKey(keyCode);
                    next += interval; // Advancing fixed interval target, will keep the effective interval absolute
                    var wait = next - stopwatch.Elapsed;
                    if (wait > TimeSpan.Zero) _cts.Token.WaitHandle.WaitOne(wait); // Will wake if canceled, then loop breaks
                }
            }
            finally
            {
                ApplyModifiers(modifiers, false); // Will run even if something errors out
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            _task?.Wait();
            Dispose();
        }

        public void Dispose() => _cts.Dispose();
    }

    private static ulong RegisterEntry(KeyboardSimulatorUtils.SimEntry entry)
    {
        var result = Services.Vr.Input.RegisterDigitalAction(
            entry.Action.Path,
            (data, info) =>
            {
                if (!data.bActive || !data.bChanged) return;

                var modifiers = entry.Modifiers.ToKeyCodes();
                switch (entry.Trigger)
                {
                    case HardwareInputTrigger.Press:
                        if (data is { bState: true })
                        {
                            ApplyModifiers(modifiers, true);
                            SimKey(entry.KeyCode);
                            ApplyModifiers(modifiers, false);
                        }

                        break;
                    case HardwareInputTrigger.Release:
                        if (data is { bState: false })
                        {
                            ApplyModifiers(modifiers, true);
                            SimKey(entry.KeyCode);
                            ApplyModifiers(modifiers, false);
                        }

                        break;
                    case HardwareInputTrigger.Held:
                        switch (data)
                        {
                            case { bState: true }:
                                ApplyModifiers(modifiers, true);
                                SimKey(entry.KeyCode, true);
                                break;
                            case { bState: false }:
                                SimKey(entry.KeyCode, false);
                                ApplyModifiers(modifiers, false);
                                break;
                        }

                        break;
                    case HardwareInputTrigger.Repeat:
                        switch (data)
                        {
                            case { bState: true }:
                            {
                                var runner = new RepeatRunner(entry.KeyCode, modifiers, entry.TriggerInterval);
                                runner.Start(); // Start immediately, before dictionary, to prevent stop before start.
                                if (!Repeaters.TryAdd(entry.GuiId, runner)) runner.Stop();
                                break;
                            }
                            case { bState: false }:
                            {
                                if (Repeaters.TryRemove(entry.GuiId, out var runner)) runner.Stop();
                                break;
                            }
                        }

                        break;
                }
            },
            entry.Action.IsChord
        );
        return result.ResultULong;
    }

    private static void UnregisterEntries(ulong[] handles)
    {
        Services.Vr.Input.UnregisterActions(handles);
        foreach (var guiId in Repeaters.Keys)
        {
            // We remove the repeaters from the dictionary so they cannot be stopped twice.
            if (Repeaters.TryRemove(guiId, out var repeater)) repeater.Stop();
        }
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