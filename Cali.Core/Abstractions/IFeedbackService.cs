namespace Cali.Core.Abstractions;

/// <summary>Rest-end feedback: vibration/haptics (+ sound), each gated by the user's settings.</summary>
public interface IFeedbackService
{
    /// <summary>Rest-end alert: vibration + sound (settings-gated).</summary>
    void Play();

    /// <summary>Light tick on set completion (haptics-gated).</summary>
    void Tick();
}
