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
        Console.WriteLine("Hello, World!");

        var server = Services.Server;
        await server.StartWebSocket(8077);

        var vr = Services.Vr;

        #region Event Registration

        vr.State += connected => Console.WriteLine("[STATE] " + (connected ? "Connected" : "Disconnected"));
        vr.DebugMessage += (message, level) => Console.WriteLine($"[DEBUG-{Enum.GetName(level)}] {message}");

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
        while (true)
        {
            if (!vr.IsInitialized()) continue;

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