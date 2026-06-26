namespace Cali.Core.Abstractions;

/// <summary>1-second cadence abstraction so timer logic is unit-testable without a UI dispatcher.</summary>
public interface ITicker
{
    event Action? Tick;
    void Start();
    void Stop();
    bool IsRunning { get; }
}
