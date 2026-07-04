namespace BVRTK.Components.Server.Backend;

public class PipeServer : AbstractServer
{
    public override Task StartOrRestart()
    {
        // CreateNamedPipe();
        throw new NotImplementedException();
    }

    public override Task Stop()
    {
        throw new NotImplementedException();
    }

    public override Task SendMessageToSingleOrAll(string? sessionId, string message)
    {
        throw new NotImplementedException();
    }

    public override Task SendMessageToSingle(string session, string message)
    {
        throw new NotImplementedException();
    }

    public override Task SendMessageToAll(string message)
    {
        throw new NotImplementedException();
    }

    public override Task SendMessageToOthers(string senderSessionId, string message)
    {
        throw new NotImplementedException();
    }

    public override Task SendMessageToGroup(string[] sessionIDs, string message)
    {
        throw new NotImplementedException();
    }
}