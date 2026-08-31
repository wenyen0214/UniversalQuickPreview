namespace UniversalQuickPreview.App.Lifecycle;

public sealed class SingleInstanceService : IDisposable
{
    private const string MutexName = @"Local\UniversalQuickPreview.Instance";
    private const string EventName = @"Local\UniversalQuickPreview.Activate";
    private readonly Mutex _mutex;
    private readonly EventWaitHandle _activationEvent;
    private readonly RegisteredWaitHandle? _registeredWait;
    private bool _disposed;

    public SingleInstanceService()
    {
        _mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
        IsPrimary = createdNew;
        _activationEvent = new EventWaitHandle(false, EventResetMode.AutoReset, EventName);

        if (IsPrimary)
        {
            _registeredWait = ThreadPool.RegisterWaitForSingleObject(
                _activationEvent,
                (_, _) => ActivationRequested?.Invoke(this, EventArgs.Empty),
                null,
                Timeout.Infinite,
                executeOnlyOnce: false);
        }
    }

    public event EventHandler? ActivationRequested;

    public bool IsPrimary { get; }

    public void SignalPrimary()
    {
        if (!IsPrimary)
        {
            _activationEvent.Set();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _registeredWait?.Unregister(null);
        _activationEvent.Dispose();
        if (IsPrimary)
        {
            _mutex.ReleaseMutex();
        }

        _mutex.Dispose();
        _disposed = true;
    }
}
