using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Server.Host;
using SuperSocket.WebSocket.Server;

namespace BVRTK.Components.Server;

/**
 * Derived from: https://github.com/BOLL7708/EasyFramework/blob/main/SuperServer.cs
 */
public sealed class SuperServer : ServerBase
{
    #region Defaults

    private const int DefaultPort = 7708;
    private const string DefaultIp = "Any";

    #endregion

    private IServer? _server;

    // We are getting crashes when loading sessions from _server directly, so we also store sessions here.
    private readonly ConcurrentDictionary<string, IAppSession?> _sessions = new();


    #region Manage

    private int _port = DefaultPort;
    private string _ip = DefaultIp;

    /// This will set the port and IP to use when the server is started.
    /// If you set this while the server is running, it needs to be restarted for them to apply.
    public void SetValues(int port, string ip = DefaultIp)
    {
        _port = port;
        _ip = ip;
    }

    public override async Task StartOrRestart()
    {
        // Stop in case of already running
        await Stop();

        // Start
        _server = Build(_port, _ip);
        _server.Options.MaxPackageLength = 100 * 1024 * 1024;
        await _server.StartAsync();
        OnStatusChanged(_server.State == ServerState.Started ? ServerStatus.Connected : ServerStatus.Error, 0);
    }

    public override async Task Stop()
    {
        if (_server != null)
        {
            foreach (var sessionPair in _sessions)
            {
                if (sessionPair.Value != null) await sessionPair.Value.CloseAsync(CloseReason.ServerShutdown);
            }

            await _server.StopAsync();
            await _server.DisposeAsync();
            _server = null;
        }

        OnStatusChanged(ServerStatus.Disconnected, 0);
    }

    #endregion

    #region Listeners

    private void Server_NewSessionConnected(IAppSession session)
    {
        _sessions[session.SessionID] = session;
        OnStatusMessage(session.SessionID, true, $"New session connected: {session.SessionID}");
        OnStatusChanged(ServerStatus.SessionCount, _sessions.Count);
    }

    private void Server_NewMessageReceived(IAppSession session, string value)
    {
        OnMessageReceived(session.SessionID, value);
        Interlocked.Increment(ref ReceivedCount);
        OnStatusChanged(ServerStatus.ReceivedCount, ReceivedCount);
    }

    private void Server_SessionClosed(IAppSession session, CloseReason reason)
    {
        _sessions.TryRemove(session.SessionID, out _);
        var reasonName = Enum.GetName(reason);
        OnStatusMessage(null, false, $"Session closed: {session.SessionID}, because: {reasonName}");
        OnStatusChanged(ServerStatus.SessionCount, _sessions.Count);
    }

    #endregion

    #region Send

    public override async Task SendMessageToSingleOrAll(string? sessionId, string message)
    {
        if (sessionId != null) await SendMessageToSingle(sessionId, message);
        else await SendMessageToAll(message);
    }

    /**
     * Send a message to a single session.
     */
    public override async Task SendMessageToSingle(string sessionId, string message)
    {
        try
        {
            if (_server is not { State: ServerState.Started })
            {
                OnServerError("Server has not started yet.");
                return;
            }
        }
        catch (ObjectDisposedException ex)
        {
            OnServerError($"Could not access server: {ex.Message}");
            return;
        }

        _sessions.TryGetValue(sessionId, out var session);
        if ((session as WebSocketSession) is { Handshaked: true })
        {
            try
            {
                if (session is not WebSocketSession webSocketSession)
                {
                    throw new Exception("Could not cast session to WebSocketSession.");
                }
                await webSocketSession.SendAsync(message);
                Interlocked.Increment(ref DeliveredCount);
                OnStatusChanged(ServerStatus.DeliveredCount, DeliveredCount);
            }
            catch (Exception ex)
            {
                OnServerError($"Failed to send message: {ex.Message}");
            }
        }
        else
        {
            OnServerError("Session is not ready, missing handshake.");
        }
    }

    /**
     * Send a message to all sessions.
     */
    public override async Task SendMessageToAll(string message)
    {
        List<Task> tasks = [];
        foreach (var sessionId in _sessions.Keys)
        {
            tasks.Add(SendMessageToSingle(sessionId, message));
        }

        await Task.WhenAll(tasks);
    }

    /**
     * Send a message to all sessions except the sender.
     */
    public override async Task SendMessageToOthers(string senderSessionId, string message)
    {
        List<Task> tasks = [];
        foreach (var sessionId in _sessions.Keys)
        {
            if (sessionId != senderSessionId) tasks.Add(SendMessageToSingle(sessionId, message));
        }

        await Task.WhenAll(tasks);
    }

    /**
     * Send a message to a group of sessions.
     */
    public override async Task SendMessageToGroup(string[] sessionIDs, string message)
    {
        List<Task> tasks = [];
        foreach (var sessionId in _sessions.Keys)
        {
            if (sessionIDs.Contains(sessionId)) tasks.Add(SendMessageToSingle(sessionId, message));
        }

        await Task.WhenAll(tasks);
    }

    #endregion

    #region BoilerPlate

    private IServer Build(int port = DefaultPort, string ip = DefaultIp)
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
                Server_NewSessionConnected(session);
                return ValueTask.CompletedTask;
            },
            (session, e) =>
            {
                Server_SessionClosed(session, e.Reason);
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
            OnServerError($"Exception from Error Handler: {exception.Message} from {session.SessionID}");
            return ValueTask.FromResult(false);
        });

        hostBuilder.ConfigureLogging((_, loggingBuilder) => { loggingBuilder.AddConsole(); });

        return hostBuilder.BuildAsServer();
    }

    #endregion
}