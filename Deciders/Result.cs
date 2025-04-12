using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System;

/// <summary>
/// A result type that can be used as a drop in replacement while waiting for the upcoming union classes: <a href="https://github.com/dotnet/csharplang/blob/main/proposals/TypeUnions.md"/>.
/// The idea is to use the native switch statement as much as possible: <a href="https://github.com/dotnet/csharplang/issues/2926"/>.
/// Should be used with the exhaustiveness checker <a href="https://github.com/shuebner/ClosedTypeHierarchyDiagnosticSuppressor"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TErr"></typeparam>
public abstract record Result<T, TErr>
    where T : notnull
    where TErr : notnull
{
    /// <summary>
    /// The private constructor in combination with sealed subtypes prevents other subtypes from existing and enables the exhaustiveness check when the nuget library <a href="https://github.com/shuebner/ClosedTypeHierarchyDiagnosticSuppressor"/> is referenced.
    /// </summary>
    private Result()
    {
    }

    public sealed record Ok(T Value) : Result<T, TErr>;

    public sealed record Error(TErr Value) : Result<T, TErr>;

    public T GetValueOrThrowException() => this switch
    {
        Ok ok => ok.Value,
        Error error => throw new InvalidOperationException($"Cannot get value from error result: ${error}")
    };

    public T? GetValueOrDefault() => this switch
    {
        Ok ok => ok.Value,
        Error => default
    };

    public TErr? GetErrorOrDefault() => this switch
    {
        Ok => default,
        Error error => error.Value
    };

    /// <summary>
    /// A temporary replacement (https://github.com/dotnet/csharplang/issues/2926) for the native `switch` statement that can be used without the need to explicitly specify the Ok/Error types.
    /// </summary>
    public TResult Switch<TResult>(Func<T, TResult> ok, Func<TErr, TResult> error)
    {
        return this switch
        {
            Ok okResult => ok(okResult.Value),
            Error errorResult => error(errorResult.Value),
        };
    }

    /// <summary>
    /// A temporary replacement (https://github.com/dotnet/csharplang/issues/2926) for the native `switch` statement that can be used without the need to explicitly specify the Ok/Error types.
    /// See <see cref="Switch{TResult}"/>, this method is for actions that do not return a value.
    /// </summary>
    public void SwitchVoid(Action<T> ok, Action<TErr> error)
    {
        switch (this)
        {
            case Error e:
                error(e.Value);
                break;
            case Ok o:
                ok(o.Value);
                break;
        }
    }

    /// <summary>
    /// Consume the result in a null-safe way, where the value or error can only be safely accessed in the corresponding if-branch.
    /// <code>
    /// Result&lt;User, string&gt; result = "error";
    ///  <para/>
    /// if (result.TryPickOk(out var user, out var error))
    /// {
    ///     var len = error.Length; // error: Dereference of a possibly null reference
    ///     var str = user.ToString(); // safe access
    /// }
    /// else
    /// {
    ///      var len = error.Length; // safe access
    ///      var str = user.ToString(); // error: Dereference of a possibly null reference
    /// }
    /// </code>
    /// </summary>
    /// <param name="value">The ok value.</param>
    /// <param name="error">The error value.</param>
    /// <returns>The bool to be used in the if statement.</returns>
    public bool TryPickOk(
        [NotNullWhen(true)] out T? value,
        [NotNullWhen(false)] out TErr? error)
    {
        switch (this)
        {
            case Ok ok:
                error = default;
                value = ok.Value;
                return true;
            case Error e:
                value = default;
                error = e.Value;
                return false;
        }

        throw new UnreachableException("All cases are handled by the switch");
    }

    /// <summary>
    /// Consume the result in a null-safe way, where the value or error can only be safely accessed in the corresponding if-branch.
    /// <code>
    /// Result&lt;User, string&gt; result = "error";
    ///  <para/>
    /// if (result.TryPickError(out var error, out var user))
    /// {
    ///    var len = error.Length; // safe access
    ///    var str = user.ToString(); // error: Dereference of a possibly null reference
    /// }
    /// else
    /// {
    ///     var len = error.Length; // error: Dereference of a possibly null reference
    ///     var str = user.ToString(); // safe access
    /// }
    /// </code>
    /// </summary>
    /// <param name="error">The ok value.</param>
    /// <param name="value">The error value.</param>
    /// <returns>The bool to be used in the if statement.</returns>
    public bool TryPickError(
        [NotNullWhen(true)] out TErr? error,
        [NotNullWhen(false)] out T? value)
    {
        switch (this)
        {
            case Error e:
                value = default;
                error = e.Value;
                return true;
            case Ok ok:
                error = default;
                value = ok.Value;
                return false;
        }

        throw new UnreachableException("All cases are handled by the switch");
    }

    /// <summary>
    /// Maps the result by applying a function to a contained <see cref="Ok"/> value, leaving an <see cref="Error"/> value untouched.
    /// Equivalent to the `map` function in other languages.
    /// <code>
    /// Result&lt;User, string&gt; result = new User();
    /// var mappedResult = result.Select(user => user.Username);
    /// </code>
    /// </summary>
    /// <param name="selector">The selector converting the ok value.</param>
    /// <typeparam name="TResult">The resulting ok type.</typeparam>
    /// <returns>The mapped result.</returns>
    public Result<TResult, TErr> Select<TResult>(Func<T, TResult> selector)
        where TResult : notnull
    {
        return this switch
        {
            Ok ok => selector(ok.Value),
            Error e => new Result<TResult, TErr>.Error(e.Value),
        };
    }

    /// <summary>
    /// Maps the result by applying a function to a contained <see cref="Error"/> value, leaving an <see cref="Ok"/> value untouched.
    /// <code>
    /// Result&lt;User, string&gt; result = User.Create(...);
    /// var mappedResult = result.SelectError(reason => new UserCreateFailed(reason));
    /// </code>
    /// </summary>
    /// <param name="selector">The selector converting the error value.</param>
    /// <typeparam name="TErrResult">The resulting error type.</typeparam>
    /// <returns>The mapped result.</returns>
    public Result<T, TErrResult> SelectError<TErrResult>(Func<TErr, TErrResult> selector)
        where TErrResult : notnull
    {
        return this switch
        {
            Ok ok => new Result<T, TErrResult>.Ok(ok.Value),
            Error e => selector(e.Value),
        };
    }

    public static implicit operator Result<T, TErr>(T value)
    {
        return new Ok(value);
    }

    public static implicit operator Result<T, TErr>(TErr value)
    {
        return new Error(value);
    }
}
