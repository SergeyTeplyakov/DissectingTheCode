namespace DtC.Episode2;

public class DataRefresher<T> : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _refreshTask;
    private T _data;
    private readonly Func<T> _fetchData;

    public DataRefresher(Func<T> fetchData)
    {
        _fetchData = fetchData;
        _data = fetchData();
        _refreshTask = Task.Run(RefreshData);
    }

    public T Data => _data;

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    ~DataRefresher()
    {
        Console.WriteLine("~DataRefresher");
        _cts?.Cancel();
    }

    private async Task RefreshData()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            _data = _fetchData();
            await Task.Delay(1000, _cts.Token);
        }
    }
}

public interface ITimerCallback
{
    void OnTimer();
}

public sealed class WeakTimer<T> : IDisposable where T : class, ITimerCallback
{
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _refreshTask;
    private readonly WeakReference<T> _target;
    private readonly TimeSpan _interval;

    public WeakTimer(T target, TimeSpan interval)
    {
        _target = new WeakReference<T>(target);
        _interval = interval;
        _refreshTask = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (CallOnTimerIfAlive())
                {
                    Console.WriteLine("Still alive");
                    await Task.Delay(_interval, _cts.Token);
                }
                else
                {
                    Console.WriteLine("Stopping the timer, since the target was GCed!");
                    _cts.Cancel();
                }
            }
        });
    }

    private bool CallOnTimerIfAlive()
    {
        // This needs to be a separate method, because otherwise
        // a local variable 't' will prevent the target from being collected.
        if (_target.TryGetTarget(out var t))
        {

            try
            {
                t.OnTimer();
                return true;

            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        return false;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

public class DataRefresherFixed : ITimerCallback
{
    private readonly WeakTimer<DataRefresherFixed> _timer;
    private byte[]? _data;
    private readonly Func<byte[]> _fetchData;

    public DataRefresherFixed(Func<byte[]> fetchData)
    {
        _fetchData = fetchData;
        _data = fetchData();
        _timer = new WeakTimer<DataRefresherFixed>(
            this,
            TimeSpan.FromSeconds(1));
    }

    public byte[] Data => _data ?? []; // Handle null case

    void ITimerCallback.OnTimer()
    {
        _data = _fetchData();
    }
}