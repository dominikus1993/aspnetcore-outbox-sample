using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RossmannPL.Common.Tests")]
namespace Sample.Api.Core.Model;

public static class Result
{
    public static Result<T> Ok<T>(T ok)
    {
        return new Success<T>(ok);
    }
    
    public static Result<T> Failure<T>(Exception exception)
    {
        return new Failure<T>(exception);
    }

    public static readonly Result<Unit> UnitResult = new Success<Unit>(Unit.Value);
}

public abstract class Result<T>
{
    internal Result()
    {
        
    }
    
    public abstract bool IsSuccess { get; }
    public abstract T Value { get; }
    public abstract Exception ErrorValue { get; }
    public abstract Result<TRes> Map<TRes>(Func<T, TRes> map);
    public abstract TRes Match<TRes>(Func<T, TRes> ok, Func<Exception, TRes> error);
    public abstract void Match(Action<T> ok, Action<Exception> error);
    public abstract object? Case { get; }
}

public sealed class Success<T>: Result<T>
{
    internal readonly T Ok;
        
    public Success(T ok)
    {
        ArgumentNullException.ThrowIfNull(ok);
        Ok = ok;
    }

    public override bool IsSuccess => true;
    public override T Value => Ok;
    public override Exception ErrorValue => throw new ValueIsSuccessException<T>(Ok);
    public override Result<TRes> Map<TRes>(Func<T, TRes> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        var res = map(Ok);
        return new Success<TRes>(res);
    }

    public override TRes Match<TRes>(Func<T, TRes> ok, Func<Exception, TRes> error)
    {
        var res = ok(Ok);
        return res;
    }

    public override void Match(Action<T> ok, Action<Exception> error)
    {
        ok(Ok);
    }

    public override object? Case => Ok;

    public void Deconstruct(out T ok)
    {
        ok = Ok;
    }
}


internal sealed class Failure<T>: Result<T>
{
    internal readonly Exception Error;
        
    public Failure(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        Error = error;
    }

    public override bool IsSuccess => false;
    public override T Value => throw new ValueIsErrorException(Error);
    public override Exception ErrorValue => Error;
    public override Result<TRes> Map<TRes>(Func<T, TRes> map)
    {
        return new Failure<TRes>(Error);
    }

    public override TRes Match<TRes>(Func<T, TRes> ok, Func<Exception, TRes> error)
    {
        var res = error(Error);
        return res;
    }

    public override void Match(Action<T> ok, Action<Exception> error)
    {
        error(ErrorValue);
    }

    public override object? Case => Error;

    public void Deconstruct(out Exception error)
    {
        error = Error;
    }
}

public sealed class ValueIsErrorException : Exception
{
    private const string ExceptionMessage = "Value is Error";
    
    public ValueIsErrorException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
    public ValueIsErrorException(Exception innerException) : base(ExceptionMessage, innerException)
    {
    }
}

public sealed class ValueIsSuccessException<T> : Exception
{
    public T CurrentValue { get; }
    
    public ValueIsSuccessException
        (T currentValue) : base("Value is Success")
    {

        CurrentValue = currentValue;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, {nameof(CurrentValue)}: {CurrentValue}";
    }
}

public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>
{
    public static readonly Unit Value = new();

    [System.Diagnostics.Contracts.Pure]
    public override int GetHashCode() =>
        0;

    [System.Diagnostics.Contracts.Pure]
    public override bool Equals(object? obj) =>
        obj is Unit;

    [System.Diagnostics.Contracts.Pure]
    public override string ToString() =>
        "()";

    [System.Diagnostics.Contracts.Pure]
    public bool Equals(Unit other) =>
        true;

    [System.Diagnostics.Contracts.Pure]
    public static bool operator ==(Unit lhs, Unit rhs) =>
        true;

    [System.Diagnostics.Contracts.Pure]
    public static bool operator !=(Unit lhs, Unit rhs) =>
        false;

    [System.Diagnostics.Contracts.Pure]
    public static bool operator >(Unit lhs, Unit rhs) =>
        false;

    [System.Diagnostics.Contracts.Pure]
    public static bool operator >=(Unit lhs, Unit rhs) =>
        true;

    [System.Diagnostics.Contracts.Pure]
    public static bool operator <(Unit lhs, Unit rhs) =>
        false;

    [System.Diagnostics.Contracts.Pure]
    public static bool operator <=(Unit lhs, Unit rhs) =>
        true;

    /// <summary>
    /// Provide an alternative value to unit
    /// </summary>
    /// <typeparam name="T">Alternative value type</typeparam>
    /// <param name="anything">Alternative value</param>
    /// <returns>Alternative value</returns>
    [System.Diagnostics.Contracts.Pure]
    public T Return<T>(T anything) => anything;

    /// <summary>
    /// Provide an alternative value to unit
    /// </summary>
    /// <typeparam name="T">Alternative value type</typeparam>
    /// <param name="anything">Alternative value</param>
    /// <returns>Alternative value</returns>
    [System.Diagnostics.Contracts.Pure]
    public T Return<T>(Func<T> anything) => anything();

    /// <summary>
    /// Always equal
    /// </summary>
    [System.Diagnostics.Contracts.Pure]
    public int CompareTo(Unit other) =>
        0;

    [System.Diagnostics.Contracts.Pure]
    public static Unit operator +(Unit a, Unit b) =>
        Value;
}
