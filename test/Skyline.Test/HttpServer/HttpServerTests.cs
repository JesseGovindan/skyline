using NUnit.Framework;
using Skyline;
using System;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Moq;

namespace Skyline.Test;

delegate Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

public class HttpServerTests
{
    HttpServer? server = null;
    [TearDown]
    public void TearDown()
    {
        if (server == null) {
            return;
        }

        server.Dispose();
    }

    [Test]
    public async Task ShouldListenOnPort_WhenStartedAsync()
    {
        // Arrange
        ushort testPort = 12345;
        server = new HttpServer(testPort);
        var shutdownHandler = Mock.Of<IDisposable>();
        var clientHandler = Mock.Of<IClientHandler>();
        // Act
        await Task.WhenAll(new Task [] {
          server.StartAsync(shutdownHandler, clientHandler),
          // Assert
          ((Func<Task>) (async () => {
            var client = await ConnectClientToAsync(testPort);
            Assert.That(client.Connected, Is.True);
            await SendTestRequestAsync(client.GetStream().WriteAsync);
          }))(),
        });
    }

    [Test]
    public async Task ShouldCallRequestHandler_WhenRequestReceivedAsync()
    {
        // Arrange
        ushort testPort = 12345;
        server = new HttpServer(testPort);
        var shutdownHandler = Mock.Of<IDisposable>();
        var clientHandler = Mock.Of<IClientHandler>();
        // Act
        await Task.WhenAll(new Task [] {
          server.StartAsync(shutdownHandler, clientHandler),
          ((Func<Task>) (async () => {
            var client = await ConnectClientToAsync(testPort);
            await SendTestRequestAsync(client.GetStream().WriteAsync);
          }))(),
        });
        // Assert
        Mock.Get(clientHandler).Verify(m => m.HandleClient(It.IsAny<StreamReader>(), It.IsAny<StreamWriter>()));
    }

    async Task<TcpClient> ConnectClientToAsync(ushort testPort)
    {
        var client = new TcpClient();
        await client.ConnectAsync(System.Net.IPAddress.Loopback, testPort);
        return client;
    }

    async Task SendTestRequestAsync(WriteAsync writer)
    {
        var buffer = System.Text.Encoding.UTF8.GetBytes("GET / HTTP/1.1");
        await writer(buffer, 0, buffer.Length, CancellationToken.None);
    }
}