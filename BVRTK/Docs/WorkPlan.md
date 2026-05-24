# Work plan for BVRTK

## Backend

1. dotnet 10 project in Rider ✅

2. EasyOpenVR 🚧
   
   1. Restructure project to have separated concerns ✅
   
   2. Integrate a pump that loads and acts on events 🚧
   
   3. Switch dependencies to use Nuget packages

3. Server implementation 🚧
   
   1. JSON-RPC-2.0 implementation 🚧
      
      1. Implement SuperServer (WebSockets) ✅
      
      2. Implement Named Pipes
      
      3. Adopt Zod for data validation
      
      4. Create test clients to ensure functionality

## Frontend

1. Get PanGui running in a project
   
   1. Use SDL3 as the backend
   
   2. Investigate if we can use SDL3 textures with SteamVR for overlays
