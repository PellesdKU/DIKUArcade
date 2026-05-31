namespace Result;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

/// <summary>
/// A type representing the result of an operation that can either produce a value of
/// type <typeparamref name="T"/>, represented by Ok(<typeparamref name="T"/>), or produce
/// an error of type <typeparamref name="E"/>, represented by Err(<typeparamref name="E"/>).
/// </summary>
/// <typeparam name="T">The type of an Ok value</typeparam>
/// <typeparam name="E">The type of an Err value</typeparam>
public readonly struct Result<T, E> {
    private readonly object value;

    public bool IsOk { get; private init; }

    public T? AsNullable => IsOk ? OkVal : default;

    private T OkVal => (T)value;
    private E ErrVal => (E)value;

    /// <summary>
    /// If contained value is Ok, execute action on it
    /// </summary>
    /// <param name="action">Action to execute</param>
    public Result<T,E> DoIfOk(Action<T> action) {
        if (IsOk) {
            action(OkVal);
        }
        return this;
    }

    /// <summary>
    /// If contained value is Err, execute action on it
    /// </summary>
    /// <param name="action">Action to execute</param>
    public Result<T,E> DoIfErr(Action<E> action) {
        if (!IsOk) {
            action(ErrVal);
        }
        return this;
    }

        /// <summary>
    /// If contained value is Ok, execute <paramref name="okAction"/> on it,
    /// otherwise, if contained value is Err, execute <paramref name="errAction"/> on that.
    /// </summary>
    /// <param name="okAction">Action to execute on Ok value</param>
    /// <param name="errAction">Action to execute on Err value</param>
    public Result<T,E> DoOrElse(Action<T> okAction, Action<E> errAction) {
        if (IsOk) {
            okAction(OkVal);
        } else {
            errAction(ErrVal);
        }
        return this;
    }

    /// <summary>
    /// Return the contained Ok value, or throw an exception if it is Err.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnwrapException" />
    public T Unwrap() => IsOk switch {
        true => OkVal,
        false => throw new UnwrapException($"{ErrVal}")
    };

    /// <summary>
    /// Transform the Result to a Result with a different Ok type by applying the <paramref name="map"/>
    /// to the contained Ok value. Useful for chaining or nesting operations that return Results
    /// with the same Err type.
    /// </summary>
    /// <typeparam name="Tn">The new Ok type</typeparam>
    /// <param name="map">Function to map contained Ok value to new Result type</param>
    /// <returns>The new Result, with the old Ok value mapped.</returns>
    public Result<Tn, E> AndThen<Tn>(Func<T, Result<Tn, E>> map) => IsOk switch {
        true => map(OkVal),
        false => new(ErrVal),
    };

    public async Task<Result<Tn, E>> AndThen<Tn>(Func<T, Task<Result<Tn, E>>> map) => IsOk switch {
        true => await map(OkVal),
        false => new(ErrVal),
    };

    /// <summary>
    /// Transform the Result to a Result with a different Err type by applying the <paramref name="map"/>
    /// to the contained Err value.
    /// </summary>
    /// <typeparam name="En">The new Err type</typeparam>
    /// <param name="map">Function to map contained Err value to new Result type</param>
    /// <returns>The new Result, with the old Err value mapped.</returns>
    public Result<T, En> OrElse<En>(Func<E, Result<T, En>> map) => IsOk switch {
        true => new(OkVal),
        false => map(ErrVal),
    };

    /// <summary>
    /// Transform the Result to a Result with different Ok and Err types by applying the appropriate map
    /// to the contained value.
    /// </summary>
    /// <typeparam name="Tn">The new Ok type</typeparam>
    /// <typeparam name="En">The new Err type</typeparam>
    /// <param name="okMap">Function to map contained Ok value to new Result type</param>
    /// <param name="errMap">Function to map contained Err value to new Result type</param>
    /// <returns>The new Result, with the old values mapped.</returns>
    public Result<Tn, En> AndThenOrElse<Tn, En>(
        Func<T, Result<Tn, En>> okMap,
        Func<E, Result<Tn, En>> errMap
    ) => IsOk switch {
        true => okMap(OkVal),
        false => errMap(ErrVal),
    };

    /// <summary>
    /// Transform the Ok type of the Result by applying <paramref name="map"/> to the contained
    /// value if said value is Ok.
    /// </summary>
    /// <typeparam name="Tn">New Ok type</typeparam>
    /// <param name="map">Map to apply to the contained Ok value</param>
    /// <returns>New Result with the Ok value mapped</returns>
    public Result<Tn, E> MapOk<Tn>(Func<T, Tn> map) => IsOk switch {
        true => new(map(OkVal)),
        false => new(ErrVal),
    };

    /// <summary>
    /// Transform the Err type of the Result by applying <paramref name="map"/> to the contained
    /// value if said value is Err.
    /// </summary>
    /// <typeparam name="En">New Err type</typeparam>
    /// <param name="map">Map to apply to the contained Err value</param>
    /// <returns>New Result with the Err value mapped</returns>
    public Result<T, En> MapErr<En>(Func<E, En> map) => IsOk switch {
        true => new(OkVal),
        false => new(map(ErrVal)),
    };

    /// <summary>
    /// Transform the Ok and Err types of the Result by applying the appropriate map.
    /// </summary>
    /// <typeparam name="Tn">New Ok type</typeparam>
    /// <typeparam name="En">New Err type</typeparam>
    /// <param name="okMap">Map to apply to the contained Ok value</param>
    /// <param name="errMap">Map to apply to the contained Err value</param>
    /// <returns>New Result with the values mapped</returns>
    public Result<Tn, En> Map<Tn, En>(Func<T, Tn> okMap, Func<E, En> errMap) {
        return value switch {
            true => new(okMap(OkVal)),
            false => new(errMap(ErrVal)),
            _ => throw new UnreachableException(),
        };
    }

    /// <summary>
    /// Map the Result to a single type <typeparamref name="U"/> by applying the appropriate
    /// map to the contained value.
    /// </summary>
    /// <typeparam name="U">The type being mapped to</typeparam>
    /// <param name="okMap">Map to apply if internal value is Ok</param>
    /// <param name="errMap">Map to apply if internal value is Err</param>
    /// <returns>Result of applying the appropriate map function.</returns>
    public U MapOrElse<U>(Func<T, U> okMap, Func<E, U> errMap) {
        return IsOk switch {
            true => okMap(OkVal),
            false => errMap(ErrVal),
        };
    }

    /// <summary>
    /// Try to get the Ok value.
    /// </summary>
    /// <param name="ok">The Ok value, if internal value is Ok</param>
    /// <returns><see langword="true"/>, if internal value is Ok, otherwise false</returns>
    public bool TryGetOk([MaybeNullWhen(false)] out T ok) {
        if (IsOk) {
            ok = OkVal;
            return true;
        }
        ok = default;
        return false;
    }

    /// <summary>
    /// Try to get the Err value.
    /// </summary>
    /// <param name="err">The Err value, if internal value is Err</param>
    /// <returns><see langword="true"/>, if internal value is Err, otherwise false</returns>
    public bool TryGetErr([MaybeNullWhen(false)] out E err) {
        if (!IsOk) {
            err = ErrVal;
            return true;
        }
        err = default;
        return false;
    }

    /// <summary>
    /// Construct an Ok Result.
    /// Used to disambiguate between constructors when E and T are the same.
    /// </summary>
    /// <typeparam name="Tt">Ok type</typeparam>
    /// <typeparam name="Et">Error type</typeparam>
    /// <param name="ok">Ok value</param>
    /// <returns>New Result of ok</returns>
    /// <exception cref="ArgumentNullException" />
    public static Result<T,E> Ok(T ok) => new(ok);

    /// <summary>
    /// Construct an Err Result.
    /// Used to disambiguate between constructors when E and T are the same.
    /// </summary>
    /// <typeparam name="Tt">Err type</typeparam>
    /// <typeparam name="Et">Error type</typeparam>
    /// <param name="err">Err value</param>
    /// <returns>New Result of err</returns>
    /// <exception cref="ArgumentNullException" />
    public static Result<T, E> Err(E err) => new(err);

    /// <summary>
    /// Construct an Ok Result.
    /// </summary>
    /// <param name="val">Ok value</param>
    public Result(T val) {
        if (val is null) { throw new ArgumentNullException(nameof(val)); }
        IsOk = true;
        value = val;
    }

    /// <summary>
    /// Construct an Err Result.
    /// </summary>
    /// <param name="err">Err value</param>
    public Result(E err) {
        if (err is null) { throw new ArgumentNullException(nameof(err)); }
        IsOk = false;
        value = err;
    }

    public override string ToString() {
        return value switch {
            T t => $"Ok({t})",
            E e => $"Err({e})",
            _ => throw new UnreachableException(),
        };
    }
}
