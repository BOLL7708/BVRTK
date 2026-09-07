using BVRTKCG.Attributes;

namespace BVRTK.Data.Setting;

[Setting]
public partial class Server
{
    [GuiCheckbox("Enabled", "Enable the server component (WebSocket) for remote access.")]
    public bool Enabled { get; set; } = true;

    [GuiIntModal(
        "WebSocket port",
        "A unique port used by the WebSocket server, is used immediately upon change.",
        64f,
        0,
        "Set & restart"
    )]
    public int Port { get; set; } = 7708;
}