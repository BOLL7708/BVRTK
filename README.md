# BOLL's VR Toolkit
This is a suite of VR extensions for SteamVR using the OpenVR API.

## Grand Plan
It is still early days, as this is a spare time project, often hampered by work stress and other distractions, but it is slowly growing. 

The overarching steps of this plan are listed below:
1. Set up a solid foundation with a [JSON-RPC 2.0](https://www.jsonrpc.org) compliant Websocket server for remote access I/O.
2. Update and streamline [EasyOpenVR](https://github.com/BOLL7708/EasyOpenVR) which is the backbone of all my OpenVR projects.
3. Implement a new texture backend that will be compatible with various graphics APIs, it is currently unclear which solution to use, but as I want to adopt [PanGui](https://pangui.io/) which currently uses [SDL3](https://github.com/libsdl-org/SDL) that will be investigated and evaluated.
   1. With PanGui, I hope to create a full interface to configure this application through a SteamVR dashboard overlay.
   2. It will also be used for anything that needs to be rendered to a texture, like screenshot overlays, notifications, HUD elements, possible other features that are unannounced.
4. Implement the Steam API for things like achievements and workshop content, any feature that can be motivated. This is secondary, but I want the experience and would love to have a deep integration.

## Current Status
1. The project has been set up with .NET 10 for multi-platform builds, no testing has been done if Linux is viable though, but it can hopefully happen at some point.
2. The WebSocket server has been set up with precompiled JSON handling, but does not have working JSON-RPC support yet.
3. The work on updating EasyOpenVR is ongoing.
4. The work on integrating SDL3 and in extension PanGui has not started yet.
5. The work on integrating the Steam API and features has not started yet.