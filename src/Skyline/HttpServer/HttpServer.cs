using System.Net.Sockets;

namespace Skyline;

public interface IClientHandler
{
    Task HandleClient(StreamReader reader, StreamWriter writer);
}

public class HttpServer: IDisposable
{
    private TcpListener server;

    public HttpServer(ushort port = 80)
    {
        server = new TcpListener(System.Net.IPAddress.Loopback, port);
    }

    public void Dispose()
    {
        server.Stop();
    }

    public async Task StartAsync(IDisposable shutdownHandler, IClientHandler streamHandler)
    {
        server.Start();
        var client = await server.AcceptTcpClientAsync();
        await Task.WhenAny(new Task[] {
            StartAsync(shutdownHandler, streamHandler),
            HandleClientAsync(client, streamHandler),
        });
    }

    private async Task HandleClientAsync(TcpClient client, IClientHandler clientHandler)
    {
       await clientHandler.HandleClient(new StreamReader(client.GetStream()), new StreamWriter(client.GetStream()));
       client.Close();
    }
}