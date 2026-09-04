using BVRTKCG.Attributes;

namespace BVRTK.Data.Setting;

[Setting]
public partial class Server
{
    [GuiCheckbox("Enabled", "Enable the server component (WebSocket) for remote access.")]
    private bool _enabled = true;
    public partial bool Enabled { get; set; }
    
    [GuiIntModal(
            "WebSocket port", 
            "A unique port used by the WebSocket server, is used immediately upon change.", 
            64f, 
            0,
            "Set & restart"
            )]
    private int _port = 7708;
    public partial int Port { get; set; }
}