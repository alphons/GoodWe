namespace GoodWe;

public class InverterError : Exception
{
    public InverterError(string message) : base(message) { }
    public InverterError(string message, Exception inner) : base(message, inner) { }
}

public class RequestFailedException : InverterError
{
    public int ConsecutiveFailuresCount { get; }
    public RequestFailedException(string message, int consecutiveFailuresCount = 0)
        : base(message) => ConsecutiveFailuresCount = consecutiveFailuresCount;
}

public class RequestRejectedException : InverterError
{
    public RequestRejectedException(string message = "Request rejected by inverter") : base(message) { }
}

public class PartialResponseException : InverterError
{
    public int Length { get; }
    public int Expected { get; }
    public PartialResponseException(int length, int expected)
        : base($"Partial response: got {length}, expected {expected}")
    {
        Length = length;
        Expected = expected;
    }
}

public class MaxRetriesException : InverterError
{
    public MaxRetriesException() : base("Maximum retries exceeded") { }
}
