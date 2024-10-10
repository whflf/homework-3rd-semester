namespace MyThreadPool;

/// <summary>
/// Represents a task that executes a function asynchronously in a thread pool and 
/// implements the <see cref="IMyTask{TResult}"/> interface.
/// </summary>
/// <typeparam name="TResult">The type of result produced by the task.</typeparam>
public class MyTask<TResult>(Func<TResult> calculation) : IMyTask<TResult>
{
    private readonly Func<TResult> _calculation = calculation ?? throw new ArgumentNullException(nameof(calculation));

    private TResult? _result;
    private Exception? _exception;
    private readonly MyThreadPool? _pool;

    private readonly object _lock = new object();

    /// <inheritdoc />
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MyTask{TResult}"/> class 
    /// with a function to be executed.
    /// </summary>
    /// <param name="calculation">The function to be executed as the task.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the provided calculation function is null.
    /// </exception>
    public MyTask(Func<TResult> calculation, MyThreadPool pool) : this(calculation)
    {
        this._pool = pool;
    }

    /// <inheritdoc />
    public TResult? Result
    {
        get
        {
            lock (this._lock)
            {
                if (!this.IsCompleted)
                {
                    Monitor.Wait(_lock);
                }

                if (this._exception is not null)
                {
                    throw new AggregateException(this._exception);
                }

                return this._result;
            }
        }
    }

    /// <summary>
    /// Executes the task by running the provided calculation function and sets 
    /// the result or captures any exception encountered.
    /// </summary>
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
            lock (_lock)
            {
                this.IsCompleted = true;
                Monitor.PulseAll(this._lock); // Уведомляет все потоки, ожидающие результат
            }
        }
    }

    /// <inheritdoc />
    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
    {
        var newCalculation = () => continuation(this.Result);
        var newTask = new MyTask<TNewResult>(newCalculation);

        lock (this._lock)
        {
            if (this._pool is not null)
            {
                this._pool.Submit(newCalculation);
            }
            else
            {
                newTask.Run();
            }
        }

        return newTask;
    }
}
