# BOLL's VR Toolkit
This is a suite of VR extensions for SteamVR using the OpenVR API.

## Plan
The goal is to combine a range of my existing [OpenVR](https://github.com/ValveSoftware/openvr) projects into one unified application and publish it to the [Steam store](https://store.steampowered.com) for free. 

It would combine:
* [OpenVR2WS](https://github.com/BOLL7708/OpenVR2WS)
* [OpenVR2Key](https://github.com/BOLL7708/OpenVR2Key)
* [OpenVRStartup](https://github.com/BOLL7708/OpenVRStartup)
* [OpenVROverlayPipe](https://github.com/BOLL7708/OpenVROverlayPipe)
* [SuperScreenShotterVR](https://github.com/BOLL7708/SuperScreenShotterVR)

This is a spare time project, often hampered by work stress and other distractions, but steady progress is being made. 

Work started at the beginning of 2026.

## Progress
Legend for the status icons:
* ✅: Completely finished
* ☑️: Integrated and works but not done 
* 🚧: Integration not complete 
* 🧪: Still resarching 
* 💤: Work has not started

The overarching steps of the development process are listed below, including an icon for the current progress status:
1. ✅ Create a logotype and application icon.
2. ☑️ Build from the ground up with .NET 10.
3. ☑️ Upgrade [EasyOpenVR](https://github.com/BOLL7708/EasyOpenVR) to be more capable.
4. ☑️ Run a [JSON-RPC 2.0](https://www.jsonrpc.org) server using [SuperSocket](https://www.supersocket.net).
5. ☑️ Using [DearImGui](https://www.dearimgui.com) with [GLFW](https://www.glfw.org) to build a dashboard interface.
6. ☑️ Construct a file based settings system that maps JSON to disk, and provides values to the GUI and systems.
7. 🚧 Steam integration for cloud sync, achievements, and more, using [Steamworks.NET](https://steamworks.github.io).
8. 💤 Create Steam store assets.
9. 💤 Transition as much as is suitable and possible from older projects to implement the prevous features.

## Links
* [Application structure plan](https://github.com/BOLL7708/BVRTK/blob/main/BVRTK/Docs/Structure.md) - Details about features to implement.
* [Workplan with detailed progress](https://github.com/BOLL7708/BVRTK/blob/main/BVRTK/Docs/WorkPlan.md) - Current work items and the status of them.