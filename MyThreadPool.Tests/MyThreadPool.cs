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
        Assert.Throws<InvalidOperationException>(() => threadPool.Submit(() => 10 * 2));
    }

    /// <summary>
    /// Tests that the correct number of threads are running in the thread pool.
    /// This test uses a <see cref="Barrier"/> to synchronize tasks.
    /// </summary>
    [Test]
    public void Test_NumberOfThreadsIsCorrect()
    {
        var threadCount = 4;
        var pool = new MyThreadPool(threadCount);

        var barrier = new Barrier(threadCount);

        var tasks = new List<IMyTask<int>>();
        for (var i = 0; i < threadCount; ++i)
        {
            var task = pool.Submit(() =>
            {
                barrier.SignalAndWait();
                return 42;
            });
            tasks.Add(task);
        }

        var extraTask = pool.Submit(() => 42);
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
        MyThreadPool pool = new MyThreadPool(2);

        var task1 = pool.Submit(() => 5);
        var task2 = pool.Submit(() => 10);
        var task3 = pool.Submit(() => 15);

        // ќжидаем, что все задачи завершатс€
        Assert.That(task1.Result, Is.EqualTo(5));
        Assert.That(task2.Result, Is.EqualTo(10));
        Assert.That(task3.Result, Is.EqualTo(15));
    }
}
