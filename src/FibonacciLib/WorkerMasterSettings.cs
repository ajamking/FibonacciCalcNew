namespace FibonacciLib;

public sealed class WorkerMasterSettings
{
    public string WorkerSlaveAdress { get; set; }
    public string RabbitMqConnectionString { get; set; }
    public int CalculationsCount { get; set; }
}
