using System;
using System.Threading.Tasks;
using BVRTK.Components.Server;
using BVRTK.Data;
using BVRTK.Utils;

namespace BVRTK;
/*
 * WebSocket server
 * OpenVR client
 * Steam client
 * GUI controller
 */
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var server = new SuperServer
        {
            statusAction = (status, i) =>
            {
                Console.WriteLine($"Action: {status.GetType().FullName} - {Enum.GetName(typeof(SuperServer.ServerStatus), i)}");
            },
            messageReceivedAction = (session, message) =>
            {
                Console.WriteLine($"MessageReceived: {session?.SessionID} - {message}");
                var settings = JsonUtils.ParseData<ApplicationSettings>(message);
                Console.WriteLine($"Settings: Value[{settings.Data?.test}],  Error[{settings.Message}]");
                Console.WriteLine($"Serialized: {JsonUtils.SerializeData(settings.Data)}");
            },
            statusMessageAction = (session, b, arg3) =>
            {
                Console.WriteLine($"MessageAction: {session?.SessionID} - {b} - {arg3}");
            }
        };

        await server.Start();
        
        while(true) {}
    }
}