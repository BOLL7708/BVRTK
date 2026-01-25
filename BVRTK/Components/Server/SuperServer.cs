using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Host;
using SuperSocket.WebSocket.Server;

namespace BVRTK.Components.Server;

/**
 * Derived from: https://github.com/BOLL7708/EasyFramework/blob/main/SuperServer.cs
 */
public class SuperServer
{
    public enum ServerStatus
    {
        Connected,
        Disconnected,
        Error,
        ReceivedCount,
        DeliveredCount,
        SessionCount
    }

    private const int DefaultPort = 7708;
    private const string DefaultIp = "Any";

    private IServer? _server;

    // We are getting crashes when loading sessions from _server directly, so we also store sessions here.
    private readonly ConcurrentDictionary<string, WebSocketSession?> _sessions = new();

    private int _deliveredCount;
    private int _receivedCount;

    #region Actions

    public Action<ServerStatus, int> statusAction = (_, _) => { Console.WriteLine("statusAction not set in server"); };
    public Action<WebSocketSession?, string> messageReceivedAction = (_, _) => { Console.WriteLine("messageReceivedAction not set in server"); };
    public Action<WebSocketSession?, bool, string> statusMessageAction = (_, _, _) => { Console.WriteLine("statusMessageAction not set in server"); };

    #endregion

    #region Manage

    public async Task Start(int port = DefaultPort, string ip = DefaultIp)
    {
        // Stop in case of already running
        await Stop();

        // Start
        _server = Build(port, ip);
        _server.Options.MaxPackageLength = 100 * 1024 * 1024;
        await _server.StartAsync();
        statusAction.Invoke(_server.State == ServerState.Started ? ServerStatus.Connected : ServerStatus.Error, 0);
    }

    public async Task Stop()
    {
        if (_server != null)
        {
            foreach (var sessionPair in _sessions)
            {
                if (sessionPair.Value != null) await sessionPair.Value.CloseAsync();
            }

            await _server.StopAsync();
            await _server.DisposeAsync();
            _server = null;
        }

        statusAction.Invoke(ServerStatus.Disconnected, 0);
    }

    #endregion

    #region Listeners

    private void Server_NewSessionConnected(WebSocketSession? session)
    {
        if (session == null) return;
        _sessions[session.SessionID] = session;
        statusMessageAction.Invoke(session, true, $"New session connected: {session.SessionID}");
        statusAction(ServerStatus.SessionCount, _sessions.Count);
    }

    private void Server_NewMessageReceived(WebSocketSession? session, string value)
    {
        messageReceivedAction.Invoke(session, value);
        Interlocked.Increment(ref _receivedCount);
        statusAction(ServerStatus.ReceivedCount, _receivedCount);
    }

    private void Server_SessionClosed(WebSocketSession? session, CloseReason reason)
    {
        if (session == null) return;
        _sessions.TryRemove(session.SessionID, out var _);
        var reasonName = Enum.GetName(typeof(CloseReason), reason);
        statusMessageAction.Invoke(null, false, $"Session closed: {session.SessionID}, because: {reasonName}");
        statusAction(ServerStatus.SessionCount, _sessions.Count);
    }

    #endregion

    #region Send

    /**
     * Send a message to a single session if provided, otherwise it will send to all sessions.
     */
    public async Task SendMessageToSingleOrAll(WebSocketSession? session, string message)
    {
        if(session != null) await SendMessageToSingle(session, message);
        else await SendMessageToAll(message);
    }
    
    /**
     * Send a message to a single session.
     */
    public async Task SendMessageToSingle(WebSocketSession session, string message)
    {
        try
        {
            if (_server is not { State: ServerState.Started }) 
            {
                Debug.WriteLine("Server has not started yet.");
                return;
            }
        }
        catch (ObjectDisposedException ex)
        {
            Debug.WriteLine($"Could not access server: {ex.Message}");
            return;
        }

        if (session is { Handshaked: true })
        {
            try
            {
                await session.SendAsync(message);
                Interlocked.Increment(ref _deliveredCount);
                statusAction(ServerStatus.DeliveredCount, _deliveredCount);
            }
            catch (Exception ex)
            {
                // TODO: Try again here?
                Debug.WriteLine($"Failed to send message: {ex.Message}");
            }
        }
        else
        {
            Debug.WriteLine("Session is not ready, missing handshake.");
        }
    }

    /**
     * Send a message to all sessions.
     */
    public async Task SendMessageToAll(string message)
    {
        List<Task> tasks = [];
        foreach (var session in _sessions.Values)
        {
            if (session != null) tasks.Add(SendMessageToSingle(session, message));
        }

        await Task.WhenAll(tasks);
    }

    /**
     * Send a message to all sessions except the sender.
     */
    public async Task SendMessageToOthers(string senderSessionId, string message)
    {
        List<Task> tasks = [];
        foreach (var session in _sessions.Values)
        {
            if (session != null && session.SessionID != senderSessionId) tasks.Add(SendMessageToSingle(session, message));
        }

        await Task.WhenAll(tasks);
    }

    /**
     * Send a message to a group of sessions.
     */
    public async Task SendMessageToGroup(string[] sessionIDs, string message)
    {
        List<Task> tasks = [];
        foreach (var session in _sessions.Values)
        {
            if (session != null && sessionIDs.Contains(session.SessionID)) tasks.Add(SendMessageToSingle(session, message));
        }

        await Task.WhenAll(tasks);
    }

    #endregion

    #region BoilerPlate

    public IServer Build(int port = DefaultPort, string ip = DefaultIp)
    {
        var hostBuilder = WebSocketHostBuilder.Create();

        hostBuilder.UseWebSocketMessageHandler((session, message) =>
        {
            Server_NewMessageReceived(session, message.Message);
            return ValueTask.CompletedTask;
        });

        hostBuilder.UseSessionHandler(
            session =>
            {
                Server_NewSessionConnected(session as WebSocketSession);
                return ValueTask.CompletedTask;
            },
            (session, e) =>
            {
                Server_SessionClosed(session as WebSocketSession ?? null, e.Reason);
                return ValueTask.CompletedTask;
            }
        );

        hostBuilder.ConfigureSuperSocket(options =>
        {
            options.AddListener(new ListenOptions
            {
                Ip = ip,
                Port = port
            });
        });

        hostBuilder.ConfigureErrorHandler((session, exception) =>
        {
            Debug.WriteLine($"Exception from Error Handler: {exception.Message} from {session.SessionID}");
            return ValueTask.FromResult(false);
        });

        hostBuilder.ConfigureLogging((_, loggingBuilder) => { loggingBuilder.AddConsole(); });

        return hostBuilder.BuildAsServer();
    }

    #endregion
}