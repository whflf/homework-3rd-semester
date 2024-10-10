namespace MyThreadPool;

/// <summary>
/// Represents an asynchronous task submitted to the thread pool.
/// Provides methods for retrieving the result and chaining additional tasks.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
public interface IMyTask<TResult>
{
    /// <summary>
    /// Gets a value indicating whether the task has completed execution.
    /// </summary>
    /// <value><c>true</c> if the task is completed; otherwise, <c>false</c>.</value>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets the result of the task. If the task has not completed, this property will block
    /// until the task finishes or throws an exception.
    /// </summary>
    /// <value>The result of the task, or <c>null</c> if the task did not produce a result.</value>
    /// <exception cref="AggregateException">Thrown if the task encounters an exception during execution.</exception>
    TResult? Result { get; }

    /// <summary>
    /// Continues the task with another task that is executed after the current task completes.
    /// The result of the current task is passed as input to the continuation function.
    /// </summary>
    /// <typeparam name="TNewResult">The type of the result produced by the continuation task.</typeparam>
    /// <param name="continuation">A function that takes the result of the current task as input
    /// and produces the result of the continuation task.</param>
    /// <returns>
    /// An <see cref="IMyTask{TNewResult}"/> that represents the continuation task.
    /// </returns>
    IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation);
}
