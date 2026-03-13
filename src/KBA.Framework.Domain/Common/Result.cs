namespace KBA.Framework.Domain.Common;

/// <summary>
/// Représente le résultat d'une opération (Pattern Result/Railway-Oriented Programming)
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? error, string? errorCode = null)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Un résultat réussi ne peut pas avoir de message d'erreur.");
        if (!isSuccess && error == null)
            throw new InvalidOperationException("Un résultat en échec doit avoir un message d'erreur.");

        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error, string? errorCode = null) => new(false, error, errorCode);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error, string? errorCode = null) => Result<T>.Failure(error, errorCode);
}

/// <summary>
/// Représente le résultat d'une opération avec une valeur de retour
/// </summary>
/// <typeparam name="T">Type de la valeur de retour</typeparam>
public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Impossible d'accéder à la valeur d'un résultat en échec.");

    protected Result(bool isSuccess, T? value, string? error, string? errorCode = null)
        : base(isSuccess, error, errorCode)
    {
        _value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public new static Result<T> Failure(string error, string? errorCode = null) => new(false, default, error, errorCode);

    public static implicit operator Result<T>(T value) => Success(value);
}
