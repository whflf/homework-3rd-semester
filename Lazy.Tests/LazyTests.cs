// <copyright file="LazyTests.cs" company="Elena Makarova">
// Copyright (c) Elena Makarova. All rights reserved.
// </copyright>

namespace Lazy.Tests;

/// <summary>
/// Contains tests for the lazy computation implementations.
/// </summary>
[TestFixture]
public class LazyTests
{
    /// <summary>
    /// Tests that both lazy implementations return the correct value.
    /// </summary>
    /// <param name="lazy">An instance of <see cref="ILazy{int}"/> to test.</param>
    [TestCaseSource(nameof(LazyObjects_ReturnsCorrectValue))]
    public void BothImplementations_ReturnsCorrectValue(ILazy<int> lazy)
    {
        Assert.That(lazy.Get(), Is.EqualTo(42));
    }

    /// <summary>
    /// Tests that both lazy implementations ensure the supplier is called only once.
    /// </summary>
    /// <param name="lazy">An instance of <see cref="ILazy{int}"/> to test.</param>
    [TestCaseSource(nameof(LazyObjects_SupplierCalledOnce))]
    public void BothImplementations_SupplierCalledOnce(ILazy<int> lazy)
    {
        lazy.Get();
        Assert.That(lazy.Get(), Is.EqualTo(0));
    }

    /// <summary>
    /// Tests that both lazy implementations handle a null return from the supplier correctly.
    /// </summary>
    /// <param name="lazy">An instance of <see cref="ILazy{object}"/> to test.</param>
    [TestCaseSource(nameof(LazyObjects_SupplierReturnsNull))]
    public void BothImplementations_SupplierReturnsNull(ILazy<object?> lazy)
    {
        Assert.That(lazy.Get(), Is.Null);
    }

    /// <summary>
    /// Tests that both lazy implementations correctly propagate exceptions thrown by the supplier.
    /// </summary>
    /// <param name="lazy">An instance of <see cref="ILazy{object}"/> to test.</param>
    [TestCaseSource(nameof(LazyObjects_SupplierThrowsException))]
    public void BothImplementations_SupplierThrowsException(ILazy<object?> lazy)
    {
        Assert.Throws<InvalidOperationException>(() => lazy.Get());
    }

    /// <summary>
    /// Tests that the multithreaded implementation handles data races correctly.
    /// </summary>
    [Test]
    public void Multithread_CheckDataRace()
    {
        var lazy = new LazyMultithread<int>(() => 1);
        var result = 0;

        var threads = new Thread[100];
        for (var i = 0; i < 100; ++i)
        {
            threads[i] = new Thread(() => result += lazy.Get());
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.That(result, Is.EqualTo(100));
    }

    /// <summary>
    /// Tests that the multithreaded implementation calls the supplier only once, even if it is slow.
    /// </summary>
    [Test]
    public void Multithread_SupplierIsSlowButCalledOnce()
    {
        var callCount = 0;
        var lazy = new LazyMultithread<int>(() =>
        {
            Thread.Sleep(500);
            ++callCount;
            return 42;
        });

        var threads = new Thread[10];
        for (var i = 0; i < 10; ++i)
        {
            threads[i] = new Thread(() => lazy.Get());
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.That(callCount, Is.EqualTo(1));
    }

    private static IEnumerable<ILazy<T>> GetLazyObjects<T>(Func<T> supplier)
    {
        yield return new LazySingleThread<T>(supplier);
        yield return new LazyMultithread<T>(supplier);
    }

    private static IEnumerable<ILazy<int>> LazyObjects_ReturnsCorrectValue()
    {
        return GetLazyObjects<int>(() => 42);
    }

    private static IEnumerable<ILazy<int>> LazyObjects_SupplierCalledOnce()
    {
        return GetLazyObjects<int>(() =>
        {
            var count = 0;
            return count++;
        });
    }

    private static IEnumerable<ILazy<object?>> LazyObjects_SupplierReturnsNull()
    {
        return GetLazyObjects<object?>(() => null);
    }

    private static IEnumerable<ILazy<object?>> LazyObjects_SupplierThrowsException()
    {
        return GetLazyObjects<object?>(() => throw new InvalidOperationException());
    }
}
