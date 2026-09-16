using System.Collections.Immutable;
using BVRTK.Components.Graphics;
using BVRTK.Components.KeyboardSimulator;
using BVRTK.Components.Server;
using BVRTK.Resources;
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
            )
            .AddActionSet(
                "keyboardsim",
                ActionSetUsage.Leftright,
                set =>
                {
                    set.AddLocalization("en_US", "Keyboard Simulator");
                    
                    // We are duplicating the hardware inputs to represent the left and right controller.
                    var hwInputsLr = Enum.GetValues<HardwareInputLeftRight>();
                    string[] prefixNames = [nameof(GeneralPrompts.Left), nameof(GeneralPrompts.Right)];
                    List<ActionGuiEntry> actionGuiEntries = [];
                    foreach (var prefixName in prefixNames)
                    {
                        foreach (var hardwareInputLeftRight in hwInputsLr)
                        {
                            var promptName = KeyboardSimulatorUtils.GetPromptNameForHardwareInputLeftRight(hardwareInputLeftRight);
                            var name = Enum.GetName(hardwareInputLeftRight);
                            if (string.IsNullOrWhiteSpace(name)) continue;
                            var prefixedName = $"{prefixName}_{name}".ToLowerInvariant(); 
                            var action = set.AddAction(
                                prefixedName,
                                requirement: ActionRequirement.Optional,
                                configure: action => { Utils.AddLocalizationsToAction(action, HardwareInputPrompts.ResourceManager, promptName, GeneralPrompts.ResourceManager, prefixName); });
                            
                            // Register the actions for display in the GUI
                            actionGuiEntries.Add(new ActionGuiEntry(prefixedName, action.Name, Utils.GetPromptWithPrefixFunc(HardwareInputPrompts.ResourceManager, promptName, GeneralPrompts.ResourceManager, prefixName)));
                        }
                    }

                    var hwInputsShared = Enum.GetValues<HardwareInputShared>();
                    foreach (var hardwareInputShared in hwInputsShared)
                    {
                        var promptName = KeyboardSimulatorUtils.GetPromptNameForHardwareInputShared(hardwareInputShared);
                        var name = Enum.GetName(hardwareInputShared)?.ToLowerInvariant();
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        var action = set.AddAction(
                            name,
                            requirement: ActionRequirement.Optional,
                            configure: action => { Utils.AddLocalizationsToAction(action, HardwareInputPrompts.ResourceManager, promptName); });
                        
                        // Register the actions for display in the GUI
                        actionGuiEntries.Add(new ActionGuiEntry(name, action.Name, Utils.GetPromptWithPrefixFunc(HardwareInputPrompts.ResourceManager, promptName)));
                    }

                    Session.GuiActionEntries = [.. actionGuiEntries];
                });
        
        
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