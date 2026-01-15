namespace SlideGenerator.Framework.Common;

/// <summary>
///     Provides lazy asynchronous initialization with thread-safe single execution guarantee and reset capability.
/// </summary>
/// <typeparam name="T">The type of object being lazily initialized.</typeparam>
public sealed class AsyncLazy<T>
{
    private readonly Func<Task<T>> _factory;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private Lazy<Task<T>>? _instance;

    /// <summary>
    ///     Initializes a new instance of AsyncLazy with a factory function.
    /// </summary>
    /// <param name="factory">The asynchronous factory function.</param>
    public AsyncLazy(Func<Task<T>> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _instance = CreateLazy();
    }

    /// <summary>
    ///     Gets a value indicating whether the value has been created.
    /// </summary>
    public bool IsValueCreated => _instance?.IsValueCreated ?? false;

    /// <summary>
    ///     Gets the lazily initialized value asynchronously.
    /// </summary>
    public Task<T> Value => _instance?.Value
                            ?? Task.FromException<T>(new ObjectDisposedException(nameof(AsyncLazy<>)));

    /// <summary>
    ///     Gets the result if available, otherwise returns default.
    /// </summary>
    public T? GetValueOrDefault()
    {
        if (!IsValueCreated) return default;
        var task = _instance?.Value;
        return task is { IsCompletedSuccessfully: true } ? task.Result : default;
    }

    /// <summary>
    ///     Resets the lazy instance, allowing re-initialization. Optionally disposes the current value.
    /// </summary>
    /// <param name="disposeAction">Optional action to dispose the current value.</param>
    public async Task ResetAsync(Action<T>? disposeAction = null)
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_instance?.IsValueCreated == true)
            {
                var task = _instance.Value;
                if (task.IsCompletedSuccessfully && disposeAction != null) disposeAction(task.Result);
            }

            _instance = CreateLazy();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    ///     Resets the lazy instance synchronously. Use with caution.
    /// </summary>
    /// <param name="disposeAction">Optional action to dispose the current value.</param>
    public void Reset(Action<T>? disposeAction = null)
    {
        _lock.Wait();
        try
        {
            if (_instance?.IsValueCreated == true)
            {
                var task = _instance.Value;
                if (task.IsCompletedSuccessfully && disposeAction != null) disposeAction(task.Result);
            }

            _instance = CreateLazy();
        }
        finally
        {
            _lock.Release();
        }
    }

    private Lazy<Task<T>> CreateLazy()
    {
        return new Lazy<Task<T>>(
            () => _factory(),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }
}