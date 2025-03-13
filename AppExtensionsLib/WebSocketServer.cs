using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AppExtensionsLib;

public class WebSocketServer
{
    private readonly ILogger<WebSocketServer> logger;

    public WebSocketServer(ILogger<WebSocketServer> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ValueWebSocketReceiveResult?> ReceiveAsync(
        WebSocket webSocket,
        Memory<byte> buffer,
        int clientId,
        CancellationToken token)
    {
        try
        {
            ValueWebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, token);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                this.logger.LogWarning("Close from client: {id}", clientId);
                return null;
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    public async Task<ValueWebSocketReceiveResult?> ReceiveWithTimeoutAsync(
        WebSocket webSocket,
        Memory<byte> buffer,
        int clientId,
        CancellationToken token,
        int timeout = 5000)
    {
        var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);

        try
        {
            timeoutCts.CancelAfter(timeout);
            ValueWebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, timeoutCts.Token);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                this.logger.LogWarning("Close from client: {id}", clientId);
                return null;
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            if (!token.IsCancellationRequested)
            {
                this.logger.LogError("Receiving timed out for client: {id}", clientId);
            }

            return null;
        }
    }

    public async Task CloseWebSocketAsync(WebSocket socket, int clientId, int closeTimeout = 1000)
    {
        try
        {
            if (socket.State == WebSocketState.Open
                || socket.State == WebSocketState.CloseReceived
                || socket.State == WebSocketState.CloseSent)
            {
                var timeoutToken = new CancellationTokenSource(closeTimeout);

                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, timeoutToken.Token);

                this.logger.LogInformation("Client disconnected: {id}", clientId);
            }
        }
        catch (OperationCanceledException)
        {
            this.logger.LogError("Closing timed out: {id}", clientId);
        }
        catch (WebSocketException ex)
        {
            this.logger.LogError("{message}", ex.Message);
        }
    }
}
