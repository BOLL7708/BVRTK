using System.Text.Json;

namespace BVRTK.Components.Server;

public class JsonRpcServer
{
    private readonly SuperServer _superServer;
    private readonly int _port;
    private readonly PipeServer _pipeServer;

    public JsonRpcServer(int port)
    {
        _port = port;
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = true
        };
        // var jsonRequest = new JsonUtils(new RequestBaseJsonSerializerContext(options));
        // var jsonResponse = new JsonUtils(new ResponseBaseJsonSerializerContext(options));
        _superServer = new SuperServer();
        _superServer.SetValues(port);
        _superServer.ServerError += Console.WriteLine;
        _superServer.StatusChanged += (status, i) => { Console.WriteLine($"Action: {status.GetType().Name} - {Enum.GetName(typeof(ServerBase.ServerStatus), i)}"); };
        _superServer.MessageReceived += async (sessionId, message) =>
        {
            Console.WriteLine($"MessageReceived: {sessionId} - {message}");
            // var settings = json.Deserialize<Settings>(message);
            // Console.WriteLine($"Settings: Value[{settings.Data?.test}],  Error[{settings.Exception?.Message ?? ""}]");
            // Console.WriteLine($"Serialized: {json.Serialize(settings.Data)}");
            if (sessionId != null) await _superServer.SendMessageToSingle(sessionId, "Welcome!");
        };
        _superServer.StatusMessage += (sessionId, newSession, message) => { Console.WriteLine($"MessageAction: {sessionId} - {newSession} - {message}"); };

        // If we could turn off without terminating we should unsubscribe from events.
    }

    public async Task Restart()
    {
        await _superServer.Restart();
    }
}