namespace FibonacciLib;

public sealed class WorkerSlaveSettings
{
    public string RabbitMqConnectionString { get; set; }
    public int ExposedPort { get; set; }
}