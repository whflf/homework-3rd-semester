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
    private readonly object _lockObject = new object();

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
        this._token.ThrowIfCancellationRequested();

        MyTask<TResult> task;
        lock (this._lockObject)
        {
            task = new MyTask<TResult>(calculation, this._token);

            this._taskQueue.Enqueue(task.Run);
            Monitor.Pulse(this._lockObject);
        }

        return task;
    }

    /// <summary>
    /// Shuts down the thread pool, preventing further task submissions and waiting 
    /// for all current tasks to complete before terminating the worker threads.
    /// </summary>
    public void Shutdown()
    {
        lock (this._lockObject)
        {
            this._source.Cancel();
            Monitor.PulseAll(this._lockObject);
        }

        foreach (var thread in this._threads)
        {
            thread.Join();
        }
    }

    private void Work()
    {
        while (!this._token.IsCancellationRequested || this._taskQueue.Count != 0)
        {
            Action? task = null;

            lock (this._lockObject)
            {
                while (!this._token.IsCancellationRequested && this._taskQueue.Count == 0)
                {
                    Monitor.Wait(this._lockObject);
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

    private class MyTask<TResult>(Func<TResult> calculation) : IMyTask<TResult>
    {
        private readonly MyThreadPool? _pool;
        private readonly CancellationToken _token;
        private readonly object _lockObject = new object();
        private readonly Func<TResult> _calculation = calculation ?? throw new ArgumentNullException(nameof(calculation));

        private TResult? _result;
        private Exception? _exception;
        private Queue<Action> _continuations = new Queue<Action>();

        public bool IsCompleted { get; private set; }

        public MyTask(Func<TResult> calculation, MyThreadPool pool) : this(calculation)
        {
            this._pool = pool;
        }

        public MyTask(Func<TResult> calculation, CancellationToken token) : this(calculation)
        {
            this._token = token;
        }

        public TResult? Result
        {
            get
            {
                if (this.IsCompleted)
                {
                    return this._result;
                }

                lock (this._lockObject)
                {
                    if (!this.IsCompleted)
                    {
                        Monitor.Wait(_lockObject);
                    }

                    if (this._exception is not null)
                    {
                        throw new AggregateException(this._exception);
                    }

                    return this._result;
                }
            }
        }

        public void Run()
        {
            try
            {
                this._result = this._calculation();
            }
            catch (Exception ex)
            {
                this._exception = ex;
            }
            finally
            {
                lock (this._lockObject)
                {
                    this.IsCompleted = true;
                    Monitor.PulseAll(this._lockObject);

                    while (this._continuations.Count > 0)
                    {
                        var continuation = this._continuations.Dequeue();
                        continuation();
                    }
                }
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
        {
            this._token.ThrowIfCancellationRequested();

            MyTask<TNewResult> newTask;

            lock (this._lockObject)
            {
                var newCalculation = () => continuation(this.Result!);
                newTask = new MyTask<TNewResult>(newCalculation, this._token);

                if (this.IsCompleted)
                {
                    this.RunContinuation(newCalculation, newTask);
                }
                else
                {
                    this._continuations.Enqueue(() =>
                    {
                        this.RunContinuation(newCalculation, newTask);
                    });
                }
            }

            return newTask;
        }

        private void RunContinuation<TNewResult>(Func<TNewResult> continuationFunction, MyTask<TNewResult> continuationTask)
        {
            if (this._pool is not null)
            {
                this._pool.Submit(continuationFunction);
            }
            else
            {
                continuationTask.Run();
            }
        }
    }
}
