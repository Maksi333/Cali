using Cali.Core.Abstractions;

namespace Cali.Platform;

public sealed class DispatcherTicker : ITicker
{
    private readonly IDispatcherTimer _timer;
    public event Action? Tick;

    public DispatcherTicker(IDispatcher dispatcher)
    {
        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.IsRepeating = true;
        _timer.Tick += (_, _) => Tick?.Invoke();
    }

    public bool IsRunning => _timer.IsRunning;
    public void Start() { if (!_timer.IsRunning) _timer.Start(); }
    public void Stop() { if (_timer.IsRunning) _timer.Stop(); }
}
