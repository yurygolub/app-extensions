using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace AppExtensionsLib;

public class WebSocketClient
{
    private readonly Uri uri;

    private readonly Action<string> logError;
    private readonly Action<string, object> logWarn;
    private readonly Action<string, object> logInfo;

    public WebSocketClient(
        Uri uri,
        Action<string> logError = null,
        Action<string, object> logWarn = null,
        Action<string, object> logInfo = null)
    {
        this.uri = uri ?? throw new ArgumentNullException(nameof(uri));

        this.logError = logError;
        this.logWarn = logWarn;
        this.logInfo = logInfo;
    }

    public async Task ConnectWebSocketAsync(ClientWebSocket socket, CancellationToken token)
    {
        await socket.ConnectAsync(this.uri, token);

        this.logInfo?.Invoke("Connected to: {uri}", this.uri);
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
                this.logWarn?.Invoke("Close from server", null);
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
                this.logWarn?.Invoke("Close from server", null);
                return null;
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            if (!token.IsCancellationRequested)
            {
                this.logError?.Invoke("Receiving timed out");
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

                this.logInfo?.Invoke("Disconnected from: {uri}", this.uri);
            }
        }
        catch (OperationCanceledException)
        {
            this.logError("Closing timed out");
        }
        catch (WebSocketException ex)
        {
            this.logError(ex.Message);
        }
    }
}
