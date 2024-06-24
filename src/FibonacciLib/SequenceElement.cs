namespace FibonacciLib;

public class SequenceElement
{
    public Guid SequenceId { get; set; }
    public long Current { get; set; }

    public SequenceElement()
    {
        SequenceId = Guid.NewGuid();
        Current = 1;
    }
}