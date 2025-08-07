using System.Buffers;
using System.Net.Sockets;

namespace TcpListenerService.Handlers;

public class TcpConnectionHandler
{
    private readonly ILogger<TcpConnectionHandler> _logger;
    private readonly TeltonikaProtocolHandler _protocol;

    public TcpConnectionHandler(ILogger<TcpConnectionHandler> logger, TeltonikaProtocolHandler protocol)
    {
        _logger = logger;
        _protocol = protocol;
    }

    public async Task HandleAsync(Socket socket, CancellationToken ct)
    {
        string imei = "";
        using var bufferOwner = MemoryPool<byte>.Shared.Rent(4096);
        var memory = bufferOwner.Memory;

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, ct);
                if (bytesRead == 0) break;

                var span = memory.Span[..bytesRead];

                var response = _protocol.Process(span, ref imei);

                foreach (var resp in response)
                {
                    await socket.SendAsync(resp, SocketFlags.None, ct);
                }

                if (_protocol.ShowDiagnostics)
                    _logger.LogDebug("Processed message from {IMEI}", imei);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Client {IMEI} disconnected or errored", imei);
        }
        finally
        {
            socket.Close();
            _logger.LogInformation("Connection closed: {IMEI}", imei);
        }
    }
}
