using TcpListenerService;
using TcpListenerService.Handlers;
using TcpListenerService.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<TcpServer>();

builder.Services.Configure<TcpServerOptions>(builder.Configuration.GetSection("TcpServer"));
builder.Services.AddSingleton<TeltonikaProtocolHandler>();
builder.Services.AddScoped<TcpConnectionHandler>();
builder.Services.AddHostedService<TcpServer>();


var host = builder.Build();
host.Run();