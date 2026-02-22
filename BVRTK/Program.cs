using System.Diagnostics;
using System.Text.Json;
using BVRTK.Components.Server;
using BVRTK.Data;
using BVRTK.Utils;
using EasyOpenVR;
using EasyOpenVR.Data;
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
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var server = new SuperServer();
        server.StatusChanged += (status, i) => { Console.WriteLine($"Action: {status.GetType().FullName} - {Enum.GetName(typeof(SuperServer.ServerStatus), i)}"); };
        server.MessageReceived += (session, message) =>
        {
            Console.WriteLine($"MessageReceived: {session?.SessionID} - {message}");
            var settings = JsonUtils.ParseData<Settings>(message);
            Console.WriteLine($"Settings: Value[{settings.Data?.test}],  Error[{settings.Message}]");
            Console.WriteLine($"Serialized: {JsonUtils.SerializeData(settings.Data)}");
        };
        server.StatusMessage += (session, newSession, message) => { Console.WriteLine($"MessageAction: {session?.SessionID} - {newSession} - {message}"); };

        // If we could turn off without terminating we should unsubscribe from events.

        await server.Start(8077);

        var application = new ApplicationBuilder("software.boll.bvrtk")
            .IsDashboardOverlay()
            .SetBinaryPathWindows("D:/Google Drive/-= BOLL7708 =-/Rider/BVRTK/BVRTK/bin/Debug/net10.0/BVRTK.exe")
            .AddStrings("en_us", new Strings("BOLL's VR Toolkit", "Suite of tools and extensions for SteamVR."))
            .Build();
        var manifestStr = new VrManifestBuilder()
            .AddApplication(application)
            .BuildAndSerialize();
        var manifestWriteTextResult = FileUtils.WriteText("software.boll.bvrtk.vrmanifest", manifestStr);
        var vr = new EasyOpenVrBuilder()
            .SetVrAppManifestPath(manifestWriteTextResult.FilePath)
            .SetApplicationType(EVRApplicationType.VRApplication_Overlay)
            .SetDebug(true)
            .BuildAndInit();

        
        Console.WriteLine("Manifest Write Test: " + manifestWriteTextResult);

        vr.State += connected => Console.WriteLine($"State: {connected}");

        vr.DebugMessage += message => Debug.WriteLine($"Debug: {message}");

        // TODO: The below likely does nothing due to initialization have barely happened.
        // var indexArr = vr.Device.GetIndexesForTrackedDeviceClass(ETrackedDeviceClass.HMD);
        // var hmdIndex = indexArr.Length > 0 ? indexArr[0] : uint.MaxValue;
        // if (hmdIndex != uint.MaxValue)
        // {
        //     var poses = vr.Device.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding);
        //     if (poses.Length > 0)
        //     {
        //         var pose = poses[0];
        //         Debug.WriteLine($"Tracking: {pose.mDeviceToAbsoluteTracking.m3}.{pose.mDeviceToAbsoluteTracking.m7}.{pose.mDeviceToAbsoluteTracking.m11}");
        //     }
        // }
        //


        while (true)
        {
        }
    }
}