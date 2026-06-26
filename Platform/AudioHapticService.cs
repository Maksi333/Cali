using Cali.Core.Abstractions;
using Cali.Core.Services;
using Microsoft.Maui.Devices;

namespace Cali.Platform;

/// <summary>
/// Rest-end feedback: vibration/haptics (Haptics setting) plus a short audible beep (Sound alerts setting).
/// The beep uses the platform tone generator on Android — no extra package, so it sidesteps the
/// CommunityToolkit.Maui.MediaElement workload-pin block.
/// </summary>
public sealed class AudioHapticService : IFeedbackService
{
    private readonly SettingsService _settings;
    public AudioHapticService(SettingsService settings) => _settings = settings;

    public void Play()
    {
        var s = _settings.Settings;
        if (s.Haptics)
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(400)); } catch { }
        }
        if (s.SoundAlerts)
            PlayBeep();
    }

    public void Tick()
    {
        if (_settings.Settings.Haptics)
            try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
    }

    // Short rest-over beep. Android only for now (iOS/Windows are no-ops until a tone path is added).
    private static void PlayBeep()
    {
#if ANDROID
        try
        {
            var tone = new Android.Media.ToneGenerator(Android.Media.Stream.Music, 80); // ~80% volume
            tone.StartTone(Android.Media.Tone.PropBeep2, 350);
            // Release shortly after the tone finishes so we don't leak the native generator.
            _ = Task.Delay(600).ContinueWith(_ => { try { tone.Release(); } catch { } });
        }
        catch { }
#endif
    }
}
