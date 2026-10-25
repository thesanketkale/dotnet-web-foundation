namespace Foundation.Domain.Exceptions;

public class ConcurrencyConflictException : DomainException
{
    public string EntityName { get; }
    public object Key { get; }

    public ConcurrencyConflictException(string entityName, object key)
        : base($"The entity '{entityName}' with key '{key}' was modified by another operation.")
    {
        EntityName = entityName;
        Key = key;
    }
}
