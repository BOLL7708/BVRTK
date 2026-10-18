using System.Globalization;
using System.Runtime.InteropServices;
using BVRTK.Components.KeyboardSimulator;
using BVRTK.Data;
using EasyOpenVR;
using Valve.VR;

namespace BVRTK;

/*
 * ✅ WebSocket server
 * ✅ OpenVR client
 * Steam client
 * GUI controller
 */
class Program
{
    static async Task Main(string[] args)
    {
        #region Settings & Version

        Settings.ReadFromDisk();

        // Console.WriteLine($"Port from disk: {Settings.Current.Server.Port}");
        // Settings.ResetToDefaults(typeof(Server));
        // Console.WriteLine($"Port after reset: {Settings.Current.Server.Port}");
        // Settings.WriteToDisk(); // Does nothing
        // Settings.Current.Server.Port = 8077;
        // Settings.Current.Server.__setDirty();
        // Settings.WriteToDisk(); // Writes the Server object to disk as it is dirty
        // Console.WriteLine($"Port after setting: {Settings.Current.Server.Port}");

        if (File.Exists("Build/version.txt"))
        {
            Session.Version = (await File.ReadAllTextAsync("Build/version.txt")).Trim();
        }

        #endregion

        var server = Services.Server; // Lazy initialization means we need to access it to launch it

        // TODO: Setup Serilog

        var vr = Services.Vr;
        var vrTask = vr.Run();

        #region Event Registration

        #region GUI

        // Updates the application language when it has changed.
        void SetLanguage(string language)
        {
            CultureInfo.CurrentUICulture = Constants.SupportedLanguages.GetValueOrDefault(language, CultureInfo.InvariantCulture);
        }

        SetLanguage(Settings.Current.Application.Language);
        SettingsChangeHandlers.OnApplicationLanguageChanged += SetLanguage;

        SettingsChangeHandlers.OnKeyboardSimulatorEntriesGeneralChanged += newValue => Console.WriteLine($"EntriesGeneral was updated: {newValue.ToArray()}");

        #endregion

        #region VR

        vr.State += state =>
        {
            if (state == EasyOpenVr.EState.QuitWasCalled)
            {
                Session.ProgramCts.Cancel();
            }
        };
        vr.DebugMessage += (message, level) => Console.WriteLine($"[DEBUG-{Enum.GetName(level)}] {message}");
        // vr.PumpCycle += Services.ApplicationWindow.Render;

        vr.Event.Register([
                EVREventType.VREvent_TrackedDeviceActivated,
                EVREventType.VREvent_TrackedDeviceDeactivated,
                EVREventType.VREvent_TrackedDeviceRoleChanged,
                EVREventType.VREvent_TrackedDeviceUpdated
            ], (in vrEvent) =>
            {
                // TODO: If enabled, output device IDs to WS.
            }
        );
        vr.Event.Register([
                EVREventType.VREvent_ChaperoneDataHasChanged,
                EVREventType.VREvent_ChaperoneUniverseHasChanged
            ], (in vrEvent) =>
            {
                // TODO: If enabled, output play area data to WS.
            }
        );

        void UpdateGame()
        {
            Session.SteamSceneAppId = vr.System.GetRunningApplicationId();
            Session.SteamSceneAppName = vr.System.GetApplicationPropertyString(Session.SteamSceneAppId, EVRApplicationProperty.Name_String);
        }

        vr.Event.Register([
                EVREventType.VREvent_SceneApplicationChanged,
                EVREventType.VREvent_SceneApplicationStateChanged
            ], (in vrEvent) =>
            {
                // TODO: If enabled, send application data to WS.
                UpdateGame();
            }
        );
        UpdateGame();


        #endregion

        #endregion

        #region Termination signal handling

        PosixSignalRegistration.Create(PosixSignal.SIGINT, OnSignal);
        PosixSignalRegistration.Create(PosixSignal.SIGTERM, OnSignal);
        PosixSignalRegistration.Create(PosixSignal.SIGQUIT, OnSignal);

        void OnSignal(PosixSignalContext ctx)
        {
            ctx.Cancel = true; // Stop forced termination
            Session.ProgramCts.Cancel(); // Perform our termination
        }

        #endregion

        var launchServicesDone = false;
        var guiTask = Task.CompletedTask;
        var cancellationToken = Session.ProgramCts.Token;

        while (!launchServicesDone)
        {
            if (!vr.IsInitialized())
            {
                Thread.Sleep(1000);
                continue;
            }

            vr.Overlay.CreateDashboardOverlay(
                Constants.Overlay.UniqueId,
                Constants.Overlay.Title,
                out var mainHandle,
                out var thumbnailHandle,
                Constants.Overlay.TextureWidth,
                Constants.Overlay.TextureHeight,
                Constants.Overlay.PhysicalWidth,
                thumbnailBytes: Utils.LoadEmbeddedResource("BVRTK.Resources.Media.bvrtk.thumbnail.png")
            );
            vr.Overlay.RegisterForOverlayEvents(mainHandle, (in vrEvent) =>
            {
                Services.GuiBackend.EnqueueOverlayEvent(in vrEvent);
            });
            
            vr.System.SetAutoLaunch(Constants.SystemApplicationKey, Settings.Current.Application.LaunchWithSteamVr);
            SettingsChangeHandlers.OnApplicationLaunchWithSteamVrChanged += (autoLaunch) => { vr.System.SetAutoLaunch(Constants.SystemApplicationKey, autoLaunch); };
            
            // region Initialize Everything
            foreach (var actionSet in Session.VrInputActionSets)
            {
                var result = vr.Input.RegisterActionSet(actionSet.Name);
                if(!result.Success) Console.WriteLine($"FAILED TO REGISTER ACTIONSET {result.ErrorName} {result.ErrorName} {result.Error}");
            }
            KeyboardSim.Init();
            Session.HapticPulseActionHandle = vr.Input.GetActionHandle(Session.HapticPulseActionName).ResultULong;
            // endregion
            
            Services.GuiBackend.SetOverlayVisible(OpenVR.Overlay.IsOverlayVisible(mainHandle));
            guiTask = Services.GuiBackend.Run(mainHandle);

            launchServicesDone = true;
        }

        try
        {
            // Initialization complete, so here we wait until the application should exit, that is, when the global cancellation token is canceled.
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // This always happens on cancel but does not catch other exceptions so we clean up in finally{}.
        }
        finally
        {
            Settings.WriteToDisk();
            await Services.Server.Stop();
            await Task.WhenAll(guiTask, vrTask); // We wait for loops and workers to finish before shutting down the SteamVR API connection.
            vr.Shutdown();
        }
    }
}