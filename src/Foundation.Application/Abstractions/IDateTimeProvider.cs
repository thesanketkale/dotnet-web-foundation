namespace Foundation.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
