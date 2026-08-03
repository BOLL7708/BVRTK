using System.Drawing;
using BVRTK.Data;
using BVRTK.Data.Setting;
using Valve.VR;
using Steamworks;

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
        Console.WriteLine("Hello, World!");

        var server = Services.Server;
        await server.StartWebSocket();
        
        #region Settings
        Settings.ReadFromDisk();
        Console.WriteLine($"Port from disk: {Settings.Current.Server.Port}");
        // Settings.ResetToDefaults(typeof(Server));
        // Console.WriteLine($"Port after reset: {Settings.Current.Server.Port}");
        // Settings.WriteToDisk(); // Does nothing
        Settings.Current.Server.Port = 8077;
        // Settings.Current.Server.__setDirty();
        Settings.WriteToDisk(); // Writes the Server object to disk as it is dirty
        Console.WriteLine($"Port after setting: {Settings.Current.Server.Port}");
        #endregion
        
        // TODO: Setup NLog

        var vr = Services.Vr;

        #region Event Registration

        vr.State += connected => Console.WriteLine("[STATE] " + (connected ? "Connected" : "Disconnected"));
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
        
        uint[] indexArr = [];
        var launchServicesDone = false;
        while (true)
        {
            if (!vr.IsInitialized()) continue;
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
                    out ulong mainHandle, 
                    out ulong thumbnailHandle, 
                    Constants.OverlayTextureWidth, 
                    Constants.OverlayTextureHeight,
                    Constants.OverlayWidth,
                    Utils.GetAbsoluteFilePath(["Resources", "Media", "bvrtk.thumbnail.png"])
                );
                vr.Overlay.RegisterForOverlayEvents(mainHandle, (in vrEvent) =>
                {
                    Services.ApplicationWindow.EnqueueOverlayEvent(in vrEvent);
                });
                
                Services.ApplicationWindow.Run(mainHandle);
            }
            if (indexArr.Length == 0) indexArr = vr.Device.GetIndexesForTrackedDeviceClass(ETrackedDeviceClass.HMD);
            var hmdIndex = indexArr.Length > 0 ? indexArr[0] : uint.MaxValue;
            if (hmdIndex == uint.MaxValue) continue;

            // var poses = vr.Device.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding);
            // if (poses.Length <= 0) continue;

            // var pose = poses[0];
            // Console.WriteLine($"Tracking HMD: {pose.mDeviceToAbsoluteTracking.m3}.{pose.mDeviceToAbsoluteTracking.m7}.{pose.mDeviceToAbsoluteTracking.m11}");

            Thread.Sleep(1000);
        }
    }
}