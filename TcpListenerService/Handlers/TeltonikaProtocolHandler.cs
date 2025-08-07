using System.Buffers.Binary;
using System.Text;
using Microsoft.Extensions.Options;
using TcpListenerService.Options;

namespace TcpListenerService.Handlers;

public class TeltonikaProtocolHandler
{
    public bool ShowDiagnostics { get; }

    public TeltonikaProtocolHandler(IOptions<TcpServerOptions> options)
    {
        ShowDiagnostics = options.Value.ShowDiagnostics;
    }

    public List<ReadOnlyMemory<byte>> Process(ReadOnlySpan<byte> span, ref string imei)
    {
        var responses = new List<ReadOnlyMemory<byte>>();

        if (IsDataPacket(span))
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(span.Slice(4, 4));
            if (span.Length < length + 12)
            {
                responses.Add(new byte[] { 0x01 });
                return responses;
            }

            var parser = new TeltonikaDeviceParser.Teltonika.TeltonikaDeviceParser(ShowDiagnostics);
            parser.OnDataReceive += Console.WriteLine;

            var data = parser.Decode(span.ToArray(), imei);
            var ack = new byte[] { 0x00, 0x00, 0x00, (byte)data.numberOfData };
            responses.Add(ack);

            parser.OnDataReceive -= Console.WriteLine;
        }
        else
        {
            imei = Encoding.ASCII.GetString(span[2..]);
            responses.Add(new byte[] { 0x01 });
        }

        return responses;
    }

    private static bool IsDataPacket(ReadOnlySpan<byte> span)
        => BinaryPrimitives.ReadInt32BigEndian(span.Slice(0, 4)) == 0;
}
