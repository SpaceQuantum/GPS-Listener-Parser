using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TcpListenerService;
using TcpListenerService.Handlers;
using TcpListenerService.Options;
using Xunit;
using Assert = Xunit.Assert;

namespace TeltonikaDeviceParser.Tests.IntegrationTests;

[Collection("Sequential")]
public class TcpServerIntegrationTests
{
    private const string IpAddress = "127.0.0.1";
    private const int Port = 5050;
    
    [Fact]
    public async Task Should_Receive_Ack_On_IMEI_Send()
    {
        using var cts = new CancellationTokenSource();
       
        var serverTask = GetServer().StartAsync(cts.Token);

        await Task.Delay(500, cts.Token); // Give server time to start

        using var client = new TcpClient();
        await client.ConnectAsync(IpAddress, Port, cts.Token);
        await using var stream = client.GetStream();

        // Sample IMEI message: 000F313233343536373839303132333435
        var hex = "000F313233343536373839303132333435";
        byte[] data = ConvertHexToBytes(hex);

        await stream.WriteAsync(data, cts.Token);
        await stream.FlushAsync(cts.Token);

        // Read response (ACK)
        var buffer = new byte[1];
        int bytesRead = await stream.ReadAsync(buffer, cts.Token);

        Assert.True(bytesRead > 0);
        Assert.Equal(0x01, buffer[0]); // ACK byte

        await cts.CancelAsync();
        await serverTask;
    }
    
    [Fact]
    public async Task Should_Receive_Process_Data()
    {
        using var cts = new CancellationTokenSource();
      
        var serverTask = GetServer().StartAsync(cts.Token);

        await Task.Delay(500, cts.Token); // Give server time to start

        using var client = new TcpClient();
        await client.ConnectAsync(IpAddress, Port, cts.Token);
        await using var stream = client.GetStream();

        // Sample IMEI message: 000F313233343536373839303132333435
        var hex = "000F313233343536373839303132333435";
        byte[] data = ConvertHexToBytes(hex);
        
        await stream.WriteAsync(data, cts.Token);
        await stream.FlushAsync(cts.Token);
        
        // Read response (ACK)
        var buffer = new byte[1];
        int bytesRead = await stream.ReadAsync(buffer, cts.Token);
        
        Assert.True(bytesRead > 0);
        Assert.Equal(0x01, buffer[0]); // ACK byte
        
        // Sample data packet
        var hexData = "00000000000000A7080400000113fc208dff000f14f650209cca80006f00d60400040004030101150316030001460000015d0000000113fc17610b000f14ffe0209cc580006e00c00500010004030101150316010001460000015e0000000113fc284945000f150f00209cd200009501080400000004030101150016030001460000015d0000000113fc267c5b000f150a50209cccc0009300680400000004030101150016030001460000015b00040000BA48";
        byte[] dataPacket = ConvertHexToBytes(hexData);

        await stream.WriteAsync(dataPacket, cts.Token);
        await stream.FlushAsync(cts.Token);

        // Read response
        var bufferData = new byte[4];
        int bytesReadData = await stream.ReadAsync(bufferData, cts.Token);

        Assert.True(bytesReadData > 0);
        Assert.Equal(0x04, bufferData[3]);

        await cts.CancelAsync();
        await serverTask;
    }

    private TcpServer GetServer()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddConsole());
        services.Configure<TcpServerOptions>(opts => opts.Port = 5050);
        services.AddSingleton<TeltonikaProtocolHandler>();
        services.AddScoped<TcpConnectionHandler>();

        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<TcpServerOptions>>();
        var logger = serviceProvider.GetRequiredService<ILogger<TcpServer>>();

        var server = new TcpServer(options, logger, serviceProvider);

        return server;
    }
    
    private static byte[] ConvertHexToBytes(string hex)
    {
        return Enumerable.Range(0, hex.Length / 2)
            .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
            .ToArray();
    }
}