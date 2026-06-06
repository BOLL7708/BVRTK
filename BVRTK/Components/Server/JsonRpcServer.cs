using System.Text.Json;
using BVRTK.Data.Request;
using BVRTK.Data.Response;

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
            var tuple = JsonHandler.DecodeMessage(message);
            await HandleResult(tuple.Item1, tuple.Item2, sessionId, ESourceServer.Websocket);
            // var settings = json.Deserialize<Settings>(message);
            // Console.WriteLine($"Settings: Value[{settings.Data?.test}],  Error[{settings.Exception?.Message ?? ""}]");
            // Console.WriteLine($"Serialized: {json.Serialize(settings.Data)}");
            // await _superServer.SendMessageToSingle(sessionId, "Welcome!");
        };
        _superServer.StatusMessage += (sessionId, newSession, message) => { Console.WriteLine($"MessageAction: {sessionId} - {newSession} - {message}"); };
        // If we could turn off without terminating we should unsubscribe from events.
    }

    public async Task Start()
    {
        await _superServer.StartOrRestart();
    }

    /// Takes parsed messages and acts upon them. Will handle responses and errors.
    private async Task HandleResult(List<JsonRpcResult> list, bool isBatch, string sessionId, ESourceServer sourceServer)
    {
        var responseList = new List<JsonRpcResponse>();

        // TODO: Should perform tasks and then build a response here if required.
        foreach (var item in list)
        {
            // TODO: PERFORM ALL THE THINGS HERE.
            //  Respond with an error body if required.
            Console.WriteLine($"BODY: {item.Result?.Id}-{item.Result?.Method}, ERR: {item.Error}");

            #region Validate
            // TODO: Figure out how to store strings including error messages later.
            //  We probably want localisation so check how resource files work.
            if (item.Result == null)
            {
                responseList.Add(new JsonRpcResponse
                {
                    Error = new JsonRpcError
                    {
                        Code = 1,
                        Message = item.Error ?? "Body of message was invalid."
                    },
                    Id = null
                });
            }
            if (item.Result?.JsonRpc != "2.0")
            {
                responseList.Add(new JsonRpcResponse
                {
                    Error = new JsonRpcError
                    {
                        Code = 2,
                        Message = "The field $.jsonrpc must be: 2.0"
                    },
                    Id = item.Result?.Id
                });
            }
            #endregion

            #region Process
            switch (item.Result?.Method)
            {
                case null:
                    responseList.Add(new JsonRpcResponse
                    {
                        Error = new JsonRpcError
                        {
                            Code = 3,
                            Message = $"No $.method supplied: {item.Result?.Method}"
                        },
                        Id = item.Result?.Id
                    });
                    break;
                case EJsonRpcMethod.ShowBindingsEditor:
                    if(item.Result?.Id != null)
                    {
                        responseList.Add(new JsonRpcResponse
                        {
                            Result = JsonSerializer.SerializeToNode(new ResultStatus
                            {
                                Done = true,
                                Message = "Successfully launched the bindings interface."
                            }, JsonRpcResponseSerializerContext.Default.ResultStatus),
                            Id = item.Result?.Id
                        });
                    }
                    break;
                default:
                    responseList.Add(new JsonRpcResponse
                    {
                        Error = new JsonRpcError
                        {
                            Code = 4,
                            Message = $"Valid but unhandled $.method supplied: {item.Result?.Method}"
                        },
                        Id = item.Result?.Id
                    });
                    break;
            }
            #endregion
        }

        if (responseList.Count == 0)
        {
            // If there is nothing to return we should not send anything.
            return;
        }

        switch (sourceServer)
        {
            case ESourceServer.Websocket:
                var message = JsonHandler.EncodeMessage(responseList, isBatch);
                await _superServer.SendMessageToSingle(sessionId, message);
                break;
            case ESourceServer.Pipe:
                break;
        }
    }
}

public enum ESourceServer
{
    Websocket,
    Pipe
}