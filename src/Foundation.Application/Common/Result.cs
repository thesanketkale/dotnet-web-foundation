namespace Foundation.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Successful result cannot have an error.");
        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failing result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => Result<T>.CreateSuccess(value);
    public static Result<T> Failure<T>(string error) => Result<T>.CreateFailure(error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failure result.");

    internal Result(bool isSuccess, T? value, string? error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    internal static Result<T> CreateSuccess(T value) => new(true, value, null);
    internal static Result<T> CreateFailure(string error) => new(false, default, error);

    public static implicit operator Result<T>(T value) => Result.Success(value);
}
