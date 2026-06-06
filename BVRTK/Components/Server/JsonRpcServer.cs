using System.Text.Json;

namespace BVRTK.Components.Server;

public class JsonRpcServer
{
    private readonly SuperServer _superServer;
    private readonly PipeServer _pipeServer;

    public JsonRpcServer(int port)
    {
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
            var tuple = JsonHandler.ParseMessage(message);
            HandleResult(tuple.Item1, tuple.Item2, sessionId, ESourceServer.Websocket);
            // var settings = json.Deserialize<Settings>(message);
            // Console.WriteLine($"Settings: Value[{settings.Data?.test}],  Error[{settings.Exception?.Message ?? ""}]");
            // Console.WriteLine($"Serialized: {json.Serialize(settings.Data)}");
            await _superServer.SendMessageToSingle(sessionId, "Welcome!");
        };
        _superServer.StatusMessage += (sessionId, newSession, message) => { Console.WriteLine($"MessageAction: {sessionId} - {newSession} - {message}"); };
        // If we could turn off without terminating we should unsubscribe from events.
    }

    public async Task Start()
    {
        await _superServer.StartOrRestart();
    }

    private void HandleResult(List<JsonRpcResult> list, bool isBatch, string sessionId, ESourceServer sourceServer)
    {
        // TODO: Should perform tasks and then build a response here if required.
        foreach (var item in list)
        {
            // TODO: PERFORM ALL THE THINGS HERE.
            //  Respond with an error body if required.
            Console.WriteLine($"OK: {item.Success}, BODY: {item.Result?.Id}-{item.Result?.Method}, ERR: {item.Error}");
        }
    }
}

public enum ESourceServer
{
    Websocket,
    Pipe
}