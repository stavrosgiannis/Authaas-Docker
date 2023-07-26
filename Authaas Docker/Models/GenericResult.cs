namespace Authaas_Docker.Models;

public class GenericResult
{
    protected GenericResult(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }

    public bool Success { get; }
    public string Message { get; }
    public bool IsFailure => !Success;

    public static GenericResult Fail(string message)
    {
        return new GenericResult(false, message);
    }

    public static GenericResult<T> Fail<T>(string message)
    {
        return new GenericResult<T>(default, false, message);
    }

    public static GenericResult Ok()
    {
        return new GenericResult(true, string.Empty);
    }

    public static GenericResult Ok(string message)
    {
        return new GenericResult(true, message);
    }

    public static GenericResult<T> Ok<T>(T value)
    {
        return new GenericResult<T>(value, true, string.Empty);
    }
}

public class GenericResult<T> : GenericResult
{
    protected internal GenericResult(T value, bool success, string message)
        : base(success, message)
    {
        Value = value;
    }

    public T Value { get; set; }
}