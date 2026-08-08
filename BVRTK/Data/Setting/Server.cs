using BVRTKCG.Attributes;

namespace BVRTK.Data.Setting;

[Setting]
public partial class Server
{
    // TODO: Create generator for input field for number with label
    private int _port = 7708;
    public partial int Port { get; set; }
}