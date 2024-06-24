using FibonacciLib;
using Microsoft.Extensions.Options;

namespace Worker1;

public class WorkerSlaveClient
{
    private readonly string _baseAdress;
    private readonly string _calculateNextRoute = "api/SequenceElements/calculate-next";

    private readonly HttpClient _workerClient;

    public WorkerSlaveClient(IOptions<WorkerMasterSettings> options)
    {
        _baseAdress = options.Value.WorkerSlaveAdress;

        _workerClient = new HttpClient();
    }

    public async Task SendSequenceElement(SequenceElement elementToSend)
    {
        WorkerLogger.LogMessage($"{_baseAdress}/{_calculateNextRoute}");

        var response = await _workerClient.PostAsJsonAsync($"{_baseAdress}/{_calculateNextRoute}", elementToSend);

        if (!response.IsSuccessStatusCode)
        {
            WorkerLogger.LogMessage($"Ошибка: {response.StatusCode}");
        }
    }
}