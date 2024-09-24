// <copyright file="LazySingleThread.cs" company="Elena Makarova">
// Copyright (c) Elena Makarova. All rights reserved.
// </copyright>

namespace Lazy;

/// <summary>
/// Provides a single-thread implementation of lazy initialization.
/// </summary>
/// <typeparam name="T">The type of the value to be lazily initialized.</typeparam>
public class LazySingleThread<T>(Func<T> supplier) : ILazy<T>
{
    private readonly Func<T> Supplier = supplier;
    private T? _value;
    private bool _valueIsCalculated = false;

    /// <summary>
    /// Gets the lazily initialized value.
    /// The first call will invoke the supplier, while subsequent calls will return the cached value.
    /// </summary>
    /// <returns>The lazily initialized value of type <typeparamref name="T"/>.</returns>
    public T? Get()
    {
        if (!this._valueIsCalculated)
        {
            this._value = this.Supplier();
            this._valueIsCalculated = true;
        }

        return this._value;
    }
}
