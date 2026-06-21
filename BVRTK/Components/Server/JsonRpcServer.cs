using System.Text.Json;
using BVRTK.Data.Request;
using BVRTK.Data.Request.Params;
using BVRTK.Data.Response;
using BVRTK.Resources;
using Valve.VR;
using static BVRTK.Data.Response.ResponseUtils.EJsonRpcErrorCode;

namespace BVRTK.Components.Server;

public class JsonRpcServer
{
    private readonly SuperServer _superServer;
    private readonly PipeServer _pipeServer;

    public JsonRpcServer()
    {
        #region WebSocket
        _superServer = new SuperServer();
        _superServer.ServerError += Console.WriteLine;
        _superServer.StatusChanged += (status, i) => { Console.WriteLine($"Action: {status.GetType().Name} - {Enum.GetName(typeof(ServerBase.ServerStatus), i)}"); };
        _superServer.MessageReceived += async (sessionId, message) =>
        {
            Console.WriteLine($"MessageReceived: {sessionId} - {message}");
            var tuple = JsonHandler.DecodeMessage(message);
            await HandleResult(tuple.Item1, tuple.Item2, sessionId, ESourceServer.Websocket);
        };
        _superServer.StatusMessage += async (sessionId, newSession, message) =>
        {
            Console.WriteLine($"MessageAction: {sessionId}, {newSession}, {message}");
            if (sessionId != null && newSession)
            {
                await HandleResponse([
                    ResponseUtils.BuildList(null, [
                        "You have connected to BVRTK!",
                        "This is a JSON-RPC 2.0 compliant server, see the official specification: https://www.jsonrpc.org/specification",
                        "The methods available can be listed calling method: ListMethods"
                    ])
                ], false, sessionId, ESourceServer.Websocket);
            }
        };
        #endregion
        
        #region NamedPipe
        _pipeServer  = new PipeServer();
        // TODO: Implement
        #endregion
    }

    #region LifeTime

    public async Task StartWebSocket(int port)
    {
        _superServer.SetValues(port);
        await _superServer.StartOrRestart();
    }

    public async Task StopWebSocket()
    {
        await _superServer.Stop();
    }

    public void StartNamedPipe()
    {
        throw new NotImplementedException();
    }

    public void StopNamedPipe()
    {
        throw new NotImplementedException();
    }

    #endregion

    private async Task HandleResult(JsonRpcResult result, string sessionId, ESourceServer sourceServer)
    {
        await HandleResult([result], false, sessionId, sourceServer);
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

            if (item.Result == null)
            {
                responseList.Add(ResponseUtils.BuildError(item, InvalidRequestBody, item.Error));
                continue;
            }

            if (item.Result?.JsonRpc != "2.0")
            {
                responseList.Add(ResponseUtils.BuildError(item, InvalidJsonRpcVersion));
                continue;
            }

            #endregion

            #region Process

            switch (item.Result.Method)
            {
                case EJsonRpcMethod.Help:
                    break;
                case EJsonRpcMethod.ListMethods:
                    if (item.Result?.Id != null)
                    {
                        // Enum.GetNames(EJsonRpcMethod);
                        responseList.Add(ResponseUtils.BuildDictionary(item.Result?.Id, new Dictionary<string, string>
                        {
                            [nameof(EJsonRpcMethod.ListMethods)] = Methods.ListMethods
                        }));
                    }

                    break;
                case EJsonRpcMethod.ShowBindingEditor: {
                    var decodedParams = JsonHandler.DecodeParamsOrDefault<ShowBindingEditor>(item.Result?.Params, ParamsJsonSerializerContext.Default.ShowBindingEditor);
                    if (!decodedParams.Success)
                    {
                        if(item.IdExists) responseList.Add(ResponseUtils.BuildError(item, InvalidBodyParams, decodedParams.Exception));
                        break;
                    }
                    var steamVrError = OpenVR.Input.OpenBindingUI("", 0, 0, decodedParams.Result.OnDesktop);
                    
                    if (!item.IdExists) break; // Notification
                    
                    if (steamVrError == EVRInputError.None)
                    {
                        responseList.Add(ResponseUtils.BuildStatus(item.Result.Id, true,
                            "Successfully launched the bindings interface.") // TODO: Localize
                        );                            
                    }
                    else
                    {
                        responseList.Add(ResponseUtils.BuildError(item, SteamVrError, new Exception(Enum.GetName(steamVrError))));
                    }
                    
                    break;
                }
                default:
                    responseList.Add(ResponseUtils.BuildError(item, UnhandledMethod));
                    break;
            }

            #endregion
        }

        await HandleResponse(responseList, isBatch, sessionId, sourceServer);
    }

    private async Task HandleResponse(List<JsonRpcResponse> responseList, bool isBatch, string sessionId, ESourceServer sourceServer)
    {
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