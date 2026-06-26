using Cali.Core.Abstractions;

namespace Cali.Tests.Fakes;

/// <summary>Deterministic ticker: tests call <see cref="Fire"/> to advance simulated seconds.</summary>
public sealed class FakeTicker : ITicker
{
    public event Action? Tick;
    public bool IsRunning { get; private set; }
    public void Start() => IsRunning = true;
    public void Stop() => IsRunning = false;
    public void Fire(int n = 1) { for (int i = 0; i < n; i++) Tick?.Invoke(); }
}
