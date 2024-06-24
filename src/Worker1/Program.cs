using EasyNetQ;
using FibonacciLib;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Worker1;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureSettings(builder);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSingleton(
            x => RabbitHutch.CreateBus(x.GetRequiredService<IOptions<WorkerMasterSettings>>().Value.RabbitMqConnectionString));
        builder.Services.AddSingleton<FibonacciSequenceProcessor>();
        builder.Services.AddSingleton<WorkerSlaveClient>();

        var app = builder.Build();

        WorkerLogger.LogMessage(JsonConvert.SerializeObject(app.Services.GetRequiredService<IOptions<WorkerMasterSettings>>()));

        await StartSequenceProcessor(app.Services);
        await StartCalculationInitializer(app.Services);

        app.Run();
    }

    public static void ConfigureSettings(WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<WorkerMasterSettings>()
            .BindConfiguration("communication")
            .Bind(builder.Configuration);
    }

    public static async Task StartSequenceProcessor(IServiceProvider container)
    {
        var processor = container.GetRequiredService<FibonacciSequenceProcessor>();

        await processor.Subscribe(container.GetRequiredService<IBus>());

        WorkerLogger.LogMessage("Worker1 подписался на получение сообщений из rabbit");
    }

    public static async Task StartCalculationInitializer(IServiceProvider container)
    {
        var sequenceProcessor = container.GetRequiredService<FibonacciSequenceProcessor>();
        var workerSlaveClient = container.GetRequiredService<WorkerSlaveClient>();

        await sequenceProcessor.InitializeCalculations(workerSlaveClient);

        WorkerLogger.LogMessage("Worker1 инициализировал расчет");
    }
}