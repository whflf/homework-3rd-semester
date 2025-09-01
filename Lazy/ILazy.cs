// <copyright file="ILazy.cs" company="Elena Makarova">
// Copyright (c) Elena Makarova. All rights reserved.
// </copyright>

namespace Lazy;

/// <summary>
/// Represents a lazy computation of a value.
/// </summary>
/// <typeparam name="T">The type of the value to be lazily computed.</typeparam>
public interface ILazy<T>
{
    /// <summary>
    /// Gets the lazily computed value.
    /// </summary>
    /// <returns>The computed value of type <typeparamref name="T"/>.</returns>
    T? Get();
}
