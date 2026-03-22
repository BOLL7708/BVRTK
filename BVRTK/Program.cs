using System.Diagnostics;
using System.Text.Json;
using BVRTK.Components.Server;
using BVRTK.Data;
using EasyOpenVR;
using EasyOpenVR.Data;
using Software.Boll.EasyUtils;
using Valve.VR;

namespace BVRTK;

/*
 * ✅ WebSocket server
 * OpenVR client
 * Steam client
 * GUI controller
 */
class Program
{
    private static readonly JsonUtils JsonUtils = new(new AppJsonSerializerContext());

    static async Task Main(string[] args)
    {
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = true
        };

        Console.WriteLine("Hello, World!");
        var server = new SuperServer();
        server.StatusChanged += (status, i) => { Console.WriteLine($"Action: {status.GetType().FullName} - {Enum.GetName(typeof(SuperServer.ServerStatus), i)}"); };
        server.MessageReceived += (session, message) =>
        {
            Console.WriteLine($"MessageReceived: {session?.SessionID} - {message}");
            var settings = JsonUtils.Deserialize<Settings>(message);
            Console.WriteLine($"Settings: Value[{settings.Data?.test}],  Error[{settings.Exception?.Message ?? ""}]");
            Console.WriteLine($"Serialized: {JsonUtils.Serialize(settings.Data)}");
        };
        server.StatusMessage += (session, newSession, message) => { Console.WriteLine($"MessageAction: {session?.SessionID} - {newSession} - {message}"); };

        // If we could turn off without terminating we should unsubscribe from events.

        await server.Start(8077);

        #region Manifest
        var vrManifestFilename = "software.boll.bvrtk.vrmanifest";
        if (!FileUtils.FileExists(vrManifestFilename).FileExists)
        {
            var application = new ApplicationBuilder("software.boll.bvrtk")
                .IsDashboardOverlay()
                .SetBinaryPathWindows("D:/Google Drive/-= BOLL7708 =-/Rider/BVRTK/BVRTK/bin/Debug/net10.0/BVRTK.exe")
                .AddStrings("en_us", new Strings("BOLL's VR Toolkit", "Suite of tools and extensions for SteamVR."))
                .Build();
            var manifestJsonResult = new VrManifestBuilder()
                .AddApplication(application)
                .BuildAndSerialize();
            Console.WriteLine(manifestJsonResult);
            var manifestWriteTextResult = FileUtils.WriteText(vrManifestFilename, manifestJsonResult.Json);
            if (!manifestWriteTextResult.Success || manifestWriteTextResult.CharsWritten == 0)
            {
                Console.WriteLine("Failed to write VR Manifest to file.");
            }
        }
        #endregion
        
        #region VR
        var vr = new EasyOpenVrBuilder()
            .SetVrAppManifestPath(vrManifestFilename)
            .SetApplicationType(EVRApplicationType.VRApplication_Overlay)
            .SetPumpInterval(EasyOpenVr.EPumpInterval.FractionOfHmdHz, 1)
            .SetDebug(true)
            .QuitWithRuntime()
            .BuildAndInit();

        #region Event Registration
        vr.State += connected => Console.WriteLine($"[STATE] {connected}");
        vr.DebugMessage += message => Console.WriteLine($"[DEBUG] {message}");
        
        vr.Event.Register((in vrEvent) =>
            {
                // TODO: If enabled, output device IDs to WS.
            },
            EVREventType.VREvent_TrackedDeviceActivated,
            EVREventType.VREvent_TrackedDeviceDeactivated,
            EVREventType.VREvent_TrackedDeviceRoleChanged,
            EVREventType.VREvent_TrackedDeviceUpdated
        );
        vr.Event.Register((in vrEvent) =>
            {
                // TODO: If enabled, output play area data to WS.
            },
            EVREventType.VREvent_ChaperoneDataHasChanged,
            EVREventType.VREvent_ChaperoneUniverseHasChanged
        );
        vr.Event.Register((in vrEvent) =>
            {
                // TODO: If enabled, send application data to WS.
            },
            EVREventType.VREvent_SceneApplicationChanged,
            EVREventType.VREvent_SceneApplicationStateChanged
        );
        #endregion
        #endregion
        
        uint[] indexArr = [];
        while (true)
        {
            if (!vr.IsInitialized()) continue;
            
            if(indexArr.Length == 0) indexArr = vr.Device.GetIndexesForTrackedDeviceClass(ETrackedDeviceClass.HMD);
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