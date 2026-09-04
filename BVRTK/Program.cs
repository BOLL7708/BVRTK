using System.Globalization;
using System.Runtime.InteropServices;
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

        #region Event Registration

        #region GUI

        // Updates the application language when it has changed.
        void SetLanguage(string language, string oldLanguage = "unused")
        {
            CultureInfo.CurrentUICulture = Constants.SupportedLanguages.GetValueOrDefault(language, CultureInfo.InvariantCulture);
        }

        SetLanguage(Settings.Current.Application.Language);
        SettingsChangeHandlers.OnApplicationLanguageChanged += SetLanguage;

        #endregion

        #region VR

        vr.State += state =>
        {
            Console.WriteLine($"[STATE] {Enum.GetName(state)}");
            if (state == EasyOpenVr.EState.ReadyToShutdown)
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
        vr.Event.Register([
                EVREventType.VREvent_SceneApplicationChanged,
                EVREventType.VREvent_SceneApplicationStateChanged
            ], (in vrEvent) =>
            {
                // TODO: If enabled, send application data to WS.
            }
        );

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

        uint[] indexArr = [];
        var launchServicesDone = false;
        var guiTask = Task.CompletedTask;

        while (!Session.ProgramCts.IsCancellationRequested)
        {
            if (!vr.IsInitialized())
            {
                Thread.Sleep(1000);
                continue;
            }

            if (!launchServicesDone)
            {
                launchServicesDone = true;

                // var result1 = vr.Overlay.CreateDashboardOverlay("bvrtk.dashboard.test.1", "BVRTK Test 1", out var mainHandle, out var thumbnailHandle);
                // var result2 = vr.Overlay.SetOverlayTextureFromFile(mainHandle, @"D:\Temp\TEST\main.jpg");
                // var result3 = vr.Overlay.SetOverlayTextureFromFile(thumbnailHandle, @"D:\Temp\TEST\thumbnail.png");
                // vr.Overlay.SetOverlayWidth(mainHandle, 2.5f);
                // Console.WriteLine($"TEST OVERLAY: {result1.Success} {result2.Success} {result3.Success}");
                var ds = Path.DirectorySeparatorChar;
                vr.Overlay.CreateDashboardOverlay(
                    Constants.OverlayUniqueId,
                    Constants.OverlayTitle,
                    out var mainHandle,
                    out var thumbnailHandle,
                    Constants.OverlayTextureWidth,
                    Constants.OverlayTextureHeight,
                    Constants.OverlayPhysicalWidth,
                    thumbnailBytes: Utils.LoadEmbeddedResource("BVRTK.Resources.Media.bvrtk.thumbnail.png")
                );
                vr.Overlay.RegisterForOverlayEvents(mainHandle, (in vrEvent) => { Services.GuiBackend.EnqueueOverlayEvent(in vrEvent); });
                Services.GuiBackend.HasTerminated += (sender, e) =>
                {
                    vr.Overlay.DestroyOverlay(mainHandle);
                    vr.Overlay.DestroyOverlay(thumbnailHandle);
                    vr.Shutdown();
                };

                vr.System.SetAutoLaunch(Constants.SystemApplicationKey, Settings.Current.Application.LaunchWithSteamVr);
                SettingsChangeHandlers.OnApplicationLaunchWithSteamVrChanged += (current, _) => { vr.System.SetAutoLaunch(Constants.SystemApplicationKey, current); };

                Services.GuiBackend.SetOverlayVisible(OpenVR.Overlay.IsOverlayVisible(mainHandle));
                guiTask = Task.Run(() => Services.GuiBackend.Run(mainHandle), Session.ProgramCts.Token);
            }

            if (indexArr.Length == 0) indexArr = vr.Device.GetIndexesForTrackedDeviceClass(ETrackedDeviceClass.HMD);
            var hmdIndex = indexArr.Length > 0 ? indexArr[0] : uint.MaxValue;
            if (hmdIndex == uint.MaxValue)
            {
                Thread.Sleep(1000);
                continue;
            }

            // var poses = vr.Device.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding);
            // if (poses.Length <= 0) continue;

            // var pose = poses[0];
            // Console.WriteLine($"Tracking HMD: {pose.mDeviceToAbsoluteTracking.m3}.{pose.mDeviceToAbsoluteTracking.m7}.{pose.mDeviceToAbsoluteTracking.m11}");

            Thread.Sleep(1000);
        }

        Settings.WriteToDisk();
        Services.Server.Stop();
        Services.GuiBackend.Terminate(); // Will trigger HasTerminated when done, which in turn finishes the shutdown for SteamVR.
        await guiTask; // We wait for that task to finish or else the GuiBackend doesn't have time to finish the termination event.
    }
}