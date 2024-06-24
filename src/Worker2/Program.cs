using EasyNetQ;
using FibonacciLib;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Worker2;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureSettings(builder);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddControllers();

        builder.Services.AddSingleton(
            x => RabbitHutch.CreateBus(x.GetRequiredService<IOptions<WorkerSlaveSettings>>().Value.RabbitMqConnectionString));

        var app = builder.Build();

        app.UseRouting();
        app.UseEndpoints(x => x.MapControllers());

        var options = app.Services.GetRequiredService<IOptions<WorkerSlaveSettings>>();
        WorkerLogger.LogMessage(JsonConvert.SerializeObject(options));

        app.Run();
    }

    public static void ConfigureSettings(WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<WorkerSlaveSettings>()
            .BindConfiguration("communication")
            .Bind(builder.Configuration);

        var exposedPort = Environment.GetEnvironmentVariable("ExposedPort");
        builder.WebHost.UseUrls($"http://*:{exposedPort}");
    }
}