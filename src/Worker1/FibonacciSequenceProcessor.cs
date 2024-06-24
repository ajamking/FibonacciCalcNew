using EasyNetQ;
using FibonacciLib;
using Microsoft.Extensions.Options;

namespace Worker1;

public class FibonacciSequenceProcessor
{
    private const int ConnectAttemptsCount = 10;
    private const int ProcessDelay = 2000;

    private readonly Dictionary<Guid, SequenceElement> _sequences;
    private readonly WorkerSlaveClient _workerSlaveClient;
    public FibonacciSequenceProcessor(IOptions<WorkerMasterSettings> options, WorkerSlaveClient workerSlaveClient)
    {
        var sequences = new Dictionary<Guid, SequenceElement>();

        for (int i = 0; i < options.Value.CalculationsCount; i++)
        {
            var sequenceElement = new SequenceElement();

            sequences.Add(sequenceElement.SequenceId, sequenceElement);
        }

        ArgumentNullException.ThrowIfNull(options);
        
        _sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
        _workerSlaveClient = workerSlaveClient;
    }

    public async Task InitializeCalculations(WorkerSlaveClient workerClient)
    {
        foreach (var sequenceElement in _sequences)
        {
            await workerClient.SendSequenceElement(sequenceElement.Value);

            WorkerLogger.LogMessage($"Worker1 sent an element to Worker2 {sequenceElement.Value.Current}");
        }
    }

    public async Task Subscribe(IBus bus)
    {
        await CheckConnection(bus);

        foreach (var sequence in _sequences)
        {
            await bus.SendReceive.ReceiveAsync<SequenceElement>(sequence.Key.ToString(), ProcesMessage);
        }
    }

    public async Task ProcesMessage(SequenceElement currentSequenceElement)
    {
        await Task.Delay(ProcessDelay);

        WorkerLogger.LogMessage($"Worker1 get the rabbit element {currentSequenceElement.Current}");

        var elementToUpdate = _sequences[currentSequenceElement.SequenceId];

        elementToUpdate.Current += currentSequenceElement.Current;

        await _workerSlaveClient.SendSequenceElement(_sequences[currentSequenceElement.SequenceId]);

        WorkerLogger.LogMessage($"Worker1 sent an element to Worker2 {elementToUpdate.Current}");
    }

    private async Task CheckConnection(IBus bus)
    {
        for (int i = 0; i <= ConnectAttemptsCount; i++)
        {
            try
            {
                await bus.Advanced.ConnectAsync();
                break;
            }
            catch (Exception)
            {
                if (i + 1 >= ConnectAttemptsCount)
                {
                    throw new Exception("Failed connect to RabbitMq");
                }

                WorkerLogger.LogMessage($"Unable connect to RabbitMq(attempt {i}). Repeat...");

                await Task.Delay(ProcessDelay);
            }
        }
    }
}