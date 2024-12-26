// <copyright file="MyThreadPoolTests.cs" company="Elena Makarova">
// Copyright (c) Elena Makarova. All rights reserved.
// </copyright>

namespace MyThreadPool.Tests;

/// <summary>
/// Contains unit tests for the <see cref="MyThreadPool"/> class.
/// These tests verify the behavior and correctness of the thread pool.
/// </summary>
public class MyThreadPoolTests
{
    /// <summary>
    /// Tests whether a submitted task returns the correct result.
    /// </summary>
    [Test]
    public void Test_ReturnsCorrectResult()
    {
        var threadPool = new MyThreadPool(4);
        var task = threadPool.Submit(() => 1 + 1);

        Assert.That(task.Result, Is.EqualTo(2));
    }

    /// <summary>
    /// Tests the execution of multiple tasks to ensure each returns the correct result.
    /// </summary>
    [Test]
    public void Test_MultipleTasksExecuteSuccessfully()
    {
        var threadPool = new MyThreadPool(4);
        var task1 = threadPool.Submit(() => 1 + 1);
        var task2 = threadPool.Submit(() => 2 + 2);
        var task3 = threadPool.Submit(() => 3 + 3);

        Assert.That(task1.Result, Is.EqualTo(2));
        Assert.That(task2.Result, Is.EqualTo(4));
        Assert.That(task3.Result, Is.EqualTo(6));

        threadPool.Shutdown();
    }

    /// <summary>
    /// Tests that an <see cref="AggregateException"/> is thrown when a task throws an exception.
    /// </summary>
    [Test]
    public void Test_ThrowsAggregateException()
    {
        var threadPool = new MyThreadPool(4);
        var task = threadPool.Submit<int>(() => throw new DivideByZeroException());

        Assert.Throws<AggregateException>(() => { var result = task.Result; });
    }

    /// <summary>
    /// Tests the <see cref="IMyTask{T}.ContinueWith"/> method, ensuring it executes after the initial task.
    /// </summary>
    [Test]
    public void Test_ContinueWithExecutesAfterTask()
    {
        var threadPool = new MyThreadPool(4);
        var task = threadPool.Submit(() => 1 + 1);

        var continuation = task.ContinueWith(result => result * 2);

        Assert.That(continuation.Result, Is.EqualTo(4));
    }

    /// <summary>
    /// Tests that the thread pool stops accepting new tasks after <see cref="MyThreadPool.Shutdown"/> is called.
    /// </summary>
    [Test]
    public void Test_ShutdownStopsThreadPool()
    {
        var threadPool = new MyThreadPool(4);
        var task = threadPool.Submit(() => 1 + 1);

        threadPool.Shutdown();

        Assert.That(task.Result, Is.EqualTo(2));
        Assert.Throws<OperationCanceledException>(() => threadPool.Submit(() => 10 * 2));
    }

    /// <summary>
    /// Tests that the correct number of threads are running in the thread pool.
    /// This test uses a <see cref="Barrier"/> to synchronize tasks.
    /// </summary>
    [Test]
    public void Test_NumberOfThreadsIsCorrect()
    {
        var threadCount = 4;
        var threadPool = new MyThreadPool(threadCount);

        var barrier = new Barrier(threadCount);

        var tasks = new List<IMyTask<int>>();
        for (var i = 0; i < threadCount; ++i)
        {
            var task = threadPool.Submit(() =>
            {
                barrier.SignalAndWait();
                return 42;
            });
            tasks.Add(task);
        }

        var extraTask = threadPool.Submit(() => 42);
        Assert.That(extraTask.IsCompleted, Is.False);

        barrier.RemoveParticipant();

        foreach (var task in tasks)
        {
            Assert.That(task.Result, Is.EqualTo(42));
        }
    }

    /// <summary>
    /// Tests that tasks in the task queue are executed when a thread becomes available.
    /// </summary>
    [Test]
    public void Test_TaskQueueExecutesWhenThreadIsFree()
    {
        var threadPool = new MyThreadPool(2);

        var task1 = threadPool.Submit(() => 5);
        var task2 = threadPool.Submit(() => 10);
        var task3 = threadPool.Submit(() => 15);

        Assert.That(task1.Result, Is.EqualTo(5));
        Assert.That(task2.Result, Is.EqualTo(10));
        Assert.That(task3.Result, Is.EqualTo(15));

        threadPool.Shutdown();
    }

    /// <summary>
    /// Tests the concurrency behavior of submitting tasks to a thread pool while shutting it down.
    /// Verifies that tasks can still complete successfully after shutdown is initiated in a separate thread.
    /// </summary>
    [Test]
    public void Test_SubmitAndShutdownConcurrency()
    {
        var threadCount = 4;
        var threadPool = new MyThreadPool(threadCount);

        var shutdownThread = new Thread(() =>
        {
            threadPool.Shutdown();
        });

        var completedTasks = 0;
        var canceledTasks = 0;

        var tasks = new List<IMyTask<int>>();
        for (int i = 0; i < threadCount * 2; ++i)
        {
            try
            {
                var task = threadPool.Submit(() => 42);
                tasks.Add(task);
            }
            catch (OperationCanceledException)
            {
                ++canceledTasks;
            }
        }

        shutdownThread.Start();

        foreach (var task in tasks)
        {
            if (task.Result == 42)
            {
                ++completedTasks;
            }
        }

        shutdownThread.Join();
        Assert.That(canceledTasks + completedTasks, Is.EqualTo(8));
    }

    /// <summary>
    /// Tests the behavior of chaining multiple continuations using <see cref="IMyTask{T}.ContinueWith"/>.
    /// Verifies that the result of each continuation is correct based on the result of the base task.
    /// </summary>
    [Test]
    public void Test_MultipleContinueWith()
    {
        var threadPool = new MyThreadPool(2);
        var baseTask = threadPool.Submit(() => 10);

        var continuationResults = new List<int>();
        var continuation1 = baseTask.ContinueWith(x => x + 1);
        var continuation2 = baseTask.ContinueWith(x => x * 2);

        continuationResults.Add(continuation1.Result);
        continuationResults.Add(continuation2.Result);

        CollectionAssert.AreEquivalent(new[] { 11, 20 }, continuationResults);

        threadPool.Shutdown();
    }

    /// <summary>
    /// Tests that a continuation task does not block if the base task has not yet completed.
    /// Verifies that the continuation task can complete successfully after the base task completes,
    /// even if a synchronization barrier is used to delay the base task's result.
    /// </summary>
    [Test]
    public void Test_ContinueWithDoesNotBlockIfBaseTaskNotComputed()
    {
        var threadPool = new MyThreadPool(2);
        var barrier = new Barrier(2);

        var baseTask = threadPool.Submit(() =>
        {
            barrier.SignalAndWait();
            return 10;
        });

        var continuationTask = baseTask.ContinueWith(x => x + 1);
        Assert.That(continuationTask, Is.Not.Null);

        barrier.RemoveParticipant();

        Assert.That(baseTask.Result, Is.EqualTo(10));
        Assert.That(continuationTask.Result, Is.EqualTo(11));

        threadPool.Shutdown();
    }
}
