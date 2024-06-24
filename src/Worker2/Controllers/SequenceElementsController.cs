using EasyNetQ;
using FibonacciLib;
using Microsoft.AspNetCore.Mvc;

namespace Worker2.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SequenceElementsController : ControllerBase
{
    private readonly IBus _bus;
    private static readonly Dictionary<Guid, SequenceElement> _previousSequenceElements;

    static SequenceElementsController()
    {
        _previousSequenceElements = new Dictionary<Guid, SequenceElement>();
    }

    public SequenceElementsController(IBus bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
    }

    [HttpPost("calculate-next")]
    public async Task PostSequenceElement(SequenceElement incomingSequenceElement, CancellationToken token)
    {
        WorkerLogger.LogMessage($"Worker2 received the element from Worker1 {incomingSequenceElement.Current}");

        EnsureSequenceElementExists(incomingSequenceElement.SequenceId);

        _previousSequenceElements[incomingSequenceElement.SequenceId].Current += incomingSequenceElement.Current;

        await SendElementToRabbitQueue(_previousSequenceElements[incomingSequenceElement.SequenceId], token);
    }

    [HttpPost("send-element-to-rabbit-queue")]
    public async Task SendElementToRabbitQueue(SequenceElement sequenceElement, CancellationToken token)
    {
        await _bus.SendReceive.SendAsync(sequenceElement.SequenceId.ToString(), sequenceElement, token);

        WorkerLogger.LogMessage($"Worker2 sent an element to rabbit {sequenceElement.Current}");
    }

    private void EnsureSequenceElementExists(Guid sequenceId)
    {
        if (!_previousSequenceElements.ContainsKey(sequenceId))
        {
            _previousSequenceElements.Add(sequenceId, new SequenceElement { SequenceId = sequenceId, Current = 0 });
        }
    }
}