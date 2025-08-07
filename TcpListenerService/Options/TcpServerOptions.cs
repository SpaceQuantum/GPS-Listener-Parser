namespace TcpListenerService.Options;

public class TcpServerOptions
{
    public string IpAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 5027;
    public int Backlog { get; set; } = 100;
    public bool ShowDiagnostics { get; set; } = false;
}
