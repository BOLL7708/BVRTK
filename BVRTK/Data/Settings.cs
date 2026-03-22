using System.Text.Json.Serialization;
using BVRTK.Components.Server;

namespace BVRTK.Data;

public class Settings
{
    public string test = "MyDefaultValue";
    public SuperServer.ServerStatus status = SuperServer.ServerStatus.Disconnected;
    
    public ApplicationSettings settings = new();
    public ServerSettings server = new();
    public KeyboardSimSettings keyboardSim = new();
    public MouseSimSettings mouseSim = new();
    public OverlaySettings overlays = new();
    public ScreenshotSettings screenshots = new();
    public PlayAreaSettings playArea = new();
    
}

public class ApplicationSettings 
{
    public bool launchWithSteamVr = true;
    public bool showTrayIcon = true;
    public bool hideFromTaskbar = true;
}

public class ServerSettings
{
    public short port = 7708;
}

public class KeyboardSimSettings
{
    
}

public class MouseSimSettings
{
    
}

public class OverlaySettings
{
    
}

public class ScreenshotSettings
{
    
}

public class PlayAreaSettings
{
    
}

[JsonSerializable(typeof(Settings))]
public partial class SettingsJsonSerializerContext : JsonSerializerContext;