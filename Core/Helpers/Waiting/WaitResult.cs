namespace Core.Helpers;

public class WaitResult<T>
{
    public bool IsSuccess { get; init; }

    public TimeSpan Elapsed { get; init; }

    public int Attempts { get; init; }

    public T? LastValue { get; init; }
}
