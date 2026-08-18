# Work plan for BVRTK
## Backend
1. ✅ .NET 10 project in Rider
2. 🚧 EasyOpenVR  
   1. ✅ Restructure project to have separated concerns
   2. ✅ Integrate a pump that loads and acts on events
   3. Switch dependencies to use Nuget packages
3. 🚧 Server implementation
   1. 🚧 JSON-RPC-2.0 implementation
      1. ✅ Implement SuperServer (WebSockets)
      2. 🚧 Implement Named Pipes
      3. Create test clients to ensure functionality
4. Steamworks

## Frontend
1. 🚧 Gui
   1. ✅ Get GLFW up and running
   2. ✅ Get DearImGui up and running
   3. ✅ Get the GLFW ImGui window to render in SteamVR as a dashboard
   4. ✅ Use SteamVR dashboard input and haptics with ImGui
   5. 🚧 Implement all the settings pages
      1. 🚧 BVRTK
      2. 🚧 Preferences
      3. 🚧 Server
      4. Keyboard Sim
      5. Mouse Sim
      6. Overlays
      7. Screenshots
      8. Play Area
      9. Events
      10. ✅ Quick settings