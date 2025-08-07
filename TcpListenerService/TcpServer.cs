using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using TcpListenerService.Handlers;
using TcpListenerService.Options;

namespace TcpListenerService;

public class TcpServer : BackgroundService
{
    private readonly TcpServerOptions _options;
    private readonly ILogger<TcpServer> _logger;
    private readonly IServiceProvider _services;
    private Socket _serverSocket;

    public TcpServer(IOptions<TcpServerOptions> options, ILogger<TcpServer> logger, IServiceProvider services)
    {
        _options = options.Value;
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var endpoint = new IPEndPoint(IPAddress.Parse(_options.IpAddress), _options.Port);
        _serverSocket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _serverSocket.Bind(endpoint);
        _serverSocket.Listen(_options.Backlog);
        
        _logger.LogInformation("Listening on {IP}:{Port}", _options.IpAddress, _options.Port);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var clientSocket = await _serverSocket.AcceptAsync(stoppingToken);

                _ = Task.Run(() =>
                {
                    using var scope = _services.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<TcpConnectionHandler>();
                    return handler.HandleAsync(clientSocket, stoppingToken);
                }, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Shutdown requested.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in ExecuteAsync");
        }
        finally
        {
            try
            {
                _serverSocket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                // Ignore if already closed
            }

            _serverSocket.Close();
            _serverSocket.Dispose();
        }
    }
}
