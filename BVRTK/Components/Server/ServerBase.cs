namespace BVRTK.Components.Server;

public abstract class ServerBase
{
    #region ServerErrorHandler

    public delegate void ServerErrorHandler(string message);

    public event ServerErrorHandler? ServerError;

    protected void OnServerError(string message)
    {
        ServerError?.Invoke(message);
    }

    #endregion

    #region ServerStatusHandler

    public delegate void ServerStatusHandler(ServerStatus status, int count);

    public event ServerStatusHandler? StatusChanged;

    protected void OnStatusChanged(ServerStatus status, int count)
    {
        StatusChanged?.Invoke(status, count);
    }

    #endregion

    #region MessageReceivedHandler

    public delegate void MessageReceivedHandler(string? sessionId, string message);

    public event MessageReceivedHandler? MessageReceived;

    protected void OnMessageReceived(string? sessionId, string message)
    {
        MessageReceived?.Invoke(sessionId, message);
    }

    #endregion

    #region StatusMessageHandler

    public delegate void StatusMessageHandler(string? sessionId, bool newSession, string message);

    public event StatusMessageHandler? StatusMessage;

    protected void OnStatusMessage(string? sessionId, bool newSession, string message)
    {
        StatusMessage?.Invoke(sessionId, newSession, message);
    }

    #endregion

    public enum ServerStatus
    {
        Connected,
        Disconnected,
        Error,
        ReceivedCount,
        DeliveredCount,
        SessionCount
    }
    
    protected int DeliveredCount;
    protected int ReceivedCount;

    #region Manage

    public abstract Task Start();
    public abstract Task Stop();
    public abstract Task Restart();

    #endregion

    #region Send

    /// Send a message to a single session if provided, otherwise it will send to all sessions.
    public abstract Task SendMessageToSingleOrAll(string? sessionId, string message);

    /// Send a message to a single session.
    public abstract Task SendMessageToSingle(string session, string message);

    /// Send a message to all sessions.
    public abstract Task SendMessageToAll(string message);

    /// Send a message to all sessions except the sender.
    public abstract Task SendMessageToOthers(string senderSessionId, string message);

    /// Send a message to a group of sessions.
    public abstract Task SendMessageToGroup(string[] sessionIDs, string message);

    #endregion
}