using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AppExtensionsLib;

public class WebSocketClient
{
    private readonly Uri uri;
    private readonly ILogger<WebSocketClient> logger;

    public WebSocketClient(Uri uri, ILogger<WebSocketClient> logger)
    {
        this.uri = uri ?? throw new ArgumentNullException(nameof(uri));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ConnectWebSocketAsync(ClientWebSocket socket, CancellationToken token)
    {
        await socket.ConnectAsync(this.uri, token);

        this.logger.LogInformation("Connected to: {uri}", this.uri);
    }

    public async ValueTask<ValueWebSocketReceiveResult?> ReceiveAsync(
        ClientWebSocket socket,
        byte[] buffer,
        CancellationToken token)
    {
        try
        {
            ValueWebSocketReceiveResult result = await socket.ReceiveAsync(new Memory<byte>(buffer), token);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                this.logger.LogWarning("Close from server");
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
        ClientWebSocket socket,
        Memory<byte> receiveBuffer,
        CancellationToken token,
        int timeout = 5000)
    {
        var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);

        try
        {
            timeoutCts.CancelAfter(timeout);
            ValueWebSocketReceiveResult result = await socket.ReceiveAsync(receiveBuffer, timeoutCts.Token);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                this.logger.LogWarning("Close from server");
                return null;
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            if (!token.IsCancellationRequested)
            {
                this.logger.LogError("Receiving timed out");
            }

            return null;
        }
    }

    public async Task CloseWebSocketAsync(ClientWebSocket socket, int closeTimeout = 1000)
    {
        try
        {
            if (socket.State == WebSocketState.Open
                || socket.State == WebSocketState.CloseReceived
                || socket.State == WebSocketState.CloseSent)
            {
                var timeoutToken = new CancellationTokenSource(closeTimeout);

                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, timeoutToken.Token);

                this.logger.LogInformation("Disconnected from: {uri}", this.uri);
            }
        }
        catch (OperationCanceledException)
        {
            this.logger.LogError("Closing timed out");
        }
        catch (WebSocketException ex)
        {
            this.logger.LogError("{msg}", ex.Message);
        }
    }
}
