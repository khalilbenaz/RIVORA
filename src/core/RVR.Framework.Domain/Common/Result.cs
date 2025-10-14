namespace RVR.Framework.Domain.Common;

/// <summary>
/// Represents the result of an operation (Result / Railway-Oriented Programming pattern).
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? ErrorCode { get; }

    /// <summary>
    /// Structured error object (available when created via Failure(Error)).
    /// </summary>
    public Error? StructuredError { get; }

    protected Result(bool isSuccess, string? error, string? errorCode = null, Error? structuredError = null)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Un résultat réussi ne peut pas avoir de message d'erreur.");
        if (!isSuccess && error == null && structuredError == null)
            throw new InvalidOperationException("Un résultat en échec doit avoir un message d'erreur.");

        IsSuccess = isSuccess;
        Error = error ?? structuredError?.Message;
        ErrorCode = errorCode ?? structuredError?.Code;
        StructuredError = structuredError;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error, string? errorCode = null) => new(false, error, errorCode);
    public static Result Failure(Error error) => new(false, null, structuredError: error);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error, string? errorCode = null) => Result<T>.Failure(error, errorCode);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);

    /// <summary>
    /// Combines multiple results. Returns success only if all results are successful.
    /// On failure, aggregates all errors into a single structured Error with InnerErrors.
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        var failures = results.Where(r => r.IsFailure).ToList();
        if (failures.Count == 0)
            return Success();

        var innerErrors = failures
            .Select(f => f.StructuredError ?? new Error(f.ErrorCode ?? "UNKNOWN", f.Error ?? "Unknown error"))
            .ToList();

        var combined = new Error(
            "COMBINED_ERRORS",
            $"{failures.Count} operation(s) failed.",
            innerErrors: innerErrors);

        return Failure(combined);
    }
}

/// <summary>
/// Represents the result of an operation with a return value.
/// </summary>
/// <typeparam name="T">The return value type.</typeparam>
public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Impossible d'accéder à la valeur d'un résultat en échec.");

    protected Result(bool isSuccess, T? value, string? error, string? errorCode = null, Error? structuredError = null)
        : base(isSuccess, error, errorCode, structuredError)
    {
        _value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public new static Result<T> Failure(string error, string? errorCode = null) => new(false, default, error, errorCode);
    public static Result<T> Failure(Error error) => new(false, default, null, structuredError: error);

    /// <summary>
    /// Transforms the success value using the provided mapping function.
    /// If the result is a failure, the error is propagated unchanged.
    /// </summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return IsSuccess
            ? Result<TOut>.Success(map(_value!))
            : StructuredError != null
                ? Result<TOut>.Failure(StructuredError)
                : Result<TOut>.Failure(Error!, ErrorCode);
    }

    /// <summary>
    /// Monadic bind (flatMap). Chains an operation that itself returns a Result.
    /// </summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        return IsSuccess
            ? bind(_value!)
            : StructuredError != null
                ? Result<TOut>.Failure(StructuredError)
                : Result<TOut>.Failure(Error!, ErrorCode);
    }

    /// <summary>
    /// Pattern matching: execute one of two functions depending on success or failure.
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsSuccess)
            return onSuccess(_value!);

        var error = StructuredError ?? new Error(ErrorCode ?? "UNKNOWN", Error ?? "Unknown error");
        return onFailure(error);
    }

    /// <summary>
    /// Implicit conversion from T to a successful Result&lt;T&gt;.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicit conversion from Error to a failed Result&lt;T&gt;.
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);
}
