using System.Collections.Immutable;
using BVRTK.Components.Graphics;
using BVRTK.Components.KeyboardSimulator;
using BVRTK.Components.Server;
using BVRTK.Resources;
using EasyOpenVR;
using EasyOpenVR.Data.Manifest;
using SharpHook.Simulation;
using Software.Boll.EasyUtils;
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

    private static readonly Lazy<EventSimulator> LazySharpHook = new(BuildSharpHook);
    public static EventSimulator SharpHook => LazySharpHook.Value;

    #endregion

    private static JsonRpcServer BuildServer()
    {
        var server = new JsonRpcServer();
        return server;
    }

    private static EasyOpenVr BuildVr()
    {
        const string dir = "_steamvr";
        
        FileUtils.EnsureDirectoryExists(dir);
        
        #region App Manifest

        const string vrManifestFilename = $"{dir}/software.boll.bvrtk.vrmanifest";
        var application = new ApplicationBuilder(Constants.SystemApplicationKey)
            .IsDashboardOverlay()
            .SetBinaryPathWindows("../BVRTK.exe")
            .AddStrings("en_us", new Strings("BOLL's VR Toolkit", "Suite of tools and extensions for SteamVR."))
            .Build();
        var vrManifestBuilder = new VrManifestBuilder()
            .AddApplication(application);

        #endregion

        #region Action Manifest

        const string actionManifestFilename = $"{dir}/software.boll.bvrtk.actions.json";
        var actionManifestBuilder = new ActionManifestBuilder()
            .AddVersion(1, 1)
            .AddDefaultBindings(ControllerType.Knuckles, $"software.boll.bvrtk.bindings.knuckles.json")
            .AddActionSet(
                Constants.ActionSet.Default,
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
                Constants.ActionSet.KeyboardSim,
                ActionSetUsage.Leftright,
                set =>
                {
                    set.AddLocalization("en_US", "Keyboard Simulator");
                    
                    // We are duplicating the hardware inputs to represent the left and right controller.
                    var hwInputsLr = Enum.GetValues<HardwareInputEnums>();
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
                            actionGuiEntries.Add(new ActionGuiEntry(
                                prefixedName, 
                                action.Name,
                                false,
                                Utils.GetPromptWithPrefixFunc(HardwareInputPrompts.ResourceManager, promptName, GeneralPrompts.ResourceManager, prefixName))
                            );
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
                        actionGuiEntries.Add(new ActionGuiEntry(
                            name, 
                            action.Name, 
                            hardwareInputShared.IsChord,
                            Utils.GetPromptWithPrefixFunc(HardwareInputPrompts.ResourceManager, promptName))
                        );
                    }

                    Session.SetActionEntriesForSet(Constants.ActionSet.KeyboardSim, [.. actionGuiEntries]);
                });
        
        
        #endregion

        // We load these so they can be registered to enable listening to inputs to those sets.
        Session.VrInputActionSets = actionManifestBuilder.GetActionSets();
        
        return new EasyOpenVrBuilder()
            .SetVrAppManifest(vrManifestFilename, vrManifestBuilder, Session.isDebug)
            .SetActionManifest(actionManifestFilename, actionManifestBuilder, Session.isDebug)
            .SetApplicationType(EVRApplicationType.VRApplication_Overlay)
            .SetPumpInterval(EasyOpenVr.EPumpInterval.FractionOfHmdHz, 1)
            .SetDebug(true)
            .BuildAndInit();
    }

    private static GuiBackend BuildApplicationWindow()
    {
        return new GuiBackend();
    }

    private static EventSimulator BuildSharpHook()
    {
        return EventSimulator.Create("BVRTK Event Simulator");
    }
}