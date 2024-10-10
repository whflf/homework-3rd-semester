namespace MyThreadPool;

/// <summary>
/// Represents a custom thread pool that manages a set of worker threads
/// and allows submitting tasks to be executed asynchronously.
/// </summary>
public class MyThreadPool
{
    private readonly Thread[] _threads;
    private readonly Queue<Action> _taskQueue = new Queue<Action>();
    private readonly CancellationTokenSource _source = new CancellationTokenSource();
    private readonly CancellationToken _token;
    private readonly object _lock = new object();

    /// <summary>
    /// Initializes a new instance of the <see cref="MyThreadPool"/> class
    /// with a specified number of worker threads.
    /// </summary>
    /// <param name="threadCount">The number of threads in the pool.</param>
    public MyThreadPool(int threadCount)
    {
        this._token = this._source.Token;
        this._threads = new Thread[threadCount];

        for (var i = 0; i < threadCount; ++i)
        {
            this._threads[i] = new Thread(Work);
            this._threads[i].Start();
        }
    }

    /// <summary>
    /// Submits a task to the thread pool for asynchronous execution.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
    /// <param name="calculation">A delegate representing the task to be executed.</param>
    /// <returns>
    /// An <see cref="IMyTask{TResult}"/> that represents the asynchronous task 
    /// and contains the result or exception information once completed.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the thread pool is in the process of shutting down.
    /// </exception>
    public IMyTask<TResult> Submit<TResult>(Func<TResult> calculation)
    {
        if (this._token.IsCancellationRequested)
        {
            throw new InvalidOperationException("ThreadPool is shutting down, cannot accept new tasks.");
        }

        var task = new MyTask<TResult>(calculation);

        lock (this._lock)
        {
            this._taskQueue.Enqueue(task.Run);
            Monitor.Pulse(this._lock);
        }

        return task;
    }

    /// <summary>
    /// Shuts down the thread pool, preventing further task submissions and waiting 
    /// for all current tasks to complete before terminating the worker threads.
    /// </summary>
    public void Shutdown()
    {
        lock (this._lock)
        {
            this._source.Cancel();
            Monitor.PulseAll(this._lock);
        }

        foreach (var thread in this._threads)
        {
            thread.Join();
        }
    }

    private void Work()
    {
        while (true)
        {
            Action? task = null;

            lock (this._lock)
            {
                while (!this._token.IsCancellationRequested && this._taskQueue.Count == 0)
                {
                    Monitor.Wait(this._lock);
                }

                if (this._token.IsCancellationRequested && this._taskQueue.Count == 0)
                {
                    this._source.Dispose();
                    return;
                }

                task = this._taskQueue.Dequeue();
            }

            task();
        }
    }
}
