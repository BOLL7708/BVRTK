using BVRTK.Components.Graphics;
using BVRTK.Components.Server;
using EasyOpenVR;
using EasyOpenVR.Data.Manifest;
using Valve.VR;

namespace BVRTK;

public static class Services
{
    #region Lazy Singletons
    private static readonly Lazy<JsonRpcServer> LazyServer = new(BuildServer);
    public static JsonRpcServer Server => LazyServer.Value;

    private static readonly Lazy<EasyOpenVr> LazyVr = new(BuildVr);
    public static EasyOpenVr Vr => LazyVr.Value;

    private static readonly Lazy<GuiBackend> LazyApplicationWindow = new(BuildApplicationWindow);
    public static GuiBackend GuiBackend => LazyApplicationWindow.Value;
    #endregion

    private static JsonRpcServer BuildServer()
    {
        var server = new JsonRpcServer();
        return server;
    }
    
    private static EasyOpenVr BuildVr()
    {
        #region App Manifest

        const string vrManifestFilename = "software.boll.bvrtk.vrmanifest";
        var application = new ApplicationBuilder(Constants.SystemApplicationKey)
            .IsDashboardOverlay()
            .SetBinaryPathWindows("D:/Google Drive/-= BOLL7708 =-/Rider/BVRTK/BVRTK/bin/Debug/net10.0/BVRTK.exe") // TODO: Figure out what this should be.
            .AddStrings("en_us", new Strings("BOLL's VR Toolkit", "Suite of tools and extensions for SteamVR."))
            .Build();
        var vrManifestBuilder = new VrManifestBuilder()
            .AddApplication(application);

        #endregion

        #region Action Manifest

        const string actionManifestFilename = "software.boll.bvrtk.actions.json";
        var actionManifestBuilder = new ActionManifestBuilder()
            .AddVersion(1, 1)
            .AddActionSet(
                "default",
                ActionSetUsage.Leftright,
                set => set
                    .AddLocalization("en-us", "Default")
                    .AddAction(
                        "test",
                        ActionType.Boolean,
                        configure: action => action.AddLocalization("EN US", "Test Input")
                    )
            );

        #endregion
        
        return new EasyOpenVrBuilder()
            .SetVrAppManifest(vrManifestFilename, vrManifestBuilder, Session.isDebug)
            .SetActionManifest(actionManifestFilename, actionManifestBuilder, Session.isDebug) // TODO: Still not working
            .SetApplicationType(EVRApplicationType.VRApplication_Overlay)
            .SetPumpInterval(EasyOpenVr.EPumpInterval.FractionOfHmdHz, 1)
            .SetDebug(true)
            .BuildAndInit();
    }

    private static GuiBackend BuildApplicationWindow()
    {
        return new GuiBackend();
    }
}