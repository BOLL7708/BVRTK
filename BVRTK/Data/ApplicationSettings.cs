using System.Text.Json.Serialization;
using BVRTK.Components.Server;

namespace BVRTK.Data;

public class ApplicationSettings
{
    public string test = "MyDefaultValue";
    public SuperServer.ServerStatus status = SuperServer.ServerStatus.Disconnected;
}