
BOLL's VR Toolkit
=================

Program
    Controller for GUI
        GUI texture
    Settings in concurrent dictionary

```  
╔════════════════╦══════════════╤═════════╤════════════╤══════════╤══════╗
║      LOGO      ║ Instructions │ Options │ Operations │ Examples │      ║
║      TYPE      ╟──────────────┴─────────┴────────────┴──────────┴──────╢ 
╠════════════════╣                                                       ║
║       Settings ║ Start with SteamVR, show tray icon, hide from taskbar ║
║         Server ║ OpenVR2WS: Port, password, various I/O options        ║
║   Keyboard Sim ║ OpenVR2Key: Maps keys to simulate from VR input       ║
║      Mouse Sim ║ New: Maps mouse events to simulate from VR input      ║
║       Overlays ║ OpenVROverlayPipe: Show overlays with various effects ║
║    Screenshots ║ SuperScreenShotterVR: Viewfinder and custom output    ║
║      Play Area ║ New: Walls around the play area that will pop up      ║
║    Run Scripts ║ OpenVRStartup: Run scripts on SteamVR launch or exit  ║
║                ║                                                       ║
║   About & Help ║ Link to website, Discord, Github                      ║
║        Version ║                                                       ║
║                ║                                                       ║
║                ║                                                       ║
╚════════════════╩═══════════════════════════════════════════════════════╝
```
Settings
--------
Listen to events from the InputController, and switch flags or set values in the settings file. We encode and write this to disk as JSON.

Provide events in turn for settings that change... monitor the file? Need to think about how to do this with the least amount of overhead and code clutter.

Server
------

Keyboard Sim
------------

Mouse Sim
---------
My personal motivation for this feature is:
* The ability to aim in a first person desktop game while using Virtual Desktop as a display overlay that is attached to the HMD. 
This means it is following head movements, and the mouse simulation will look around in the game to match the head motion.
* It could also be used to control a mouse cursor with other inputs, like joysticks or trackpads or the direction of a controller.
Will have to avoid scope creep here, and add things that are actually requested and motivated, to avoid overcomplication.

Minimal viable product:
* Be able to set how to simulate mouse actions per Steam app ID, like for key mappings. Also provide a default one.
* Provide various options for how to simulate mouse movement.
  * Absolute or relative positioning
    * Start with relative, not sure absolute makes sense.
  * Option to invert X or Y movement.
* Input
  * Headset or controller rotation
    * Angle to distance in degrees to pixels
    * Compensate for roll, for when the display is following the HMD or controller.
    * An action to reset 
  * Joystick

Overlays
--------

Screenshots
-----------

Area Walls
----------