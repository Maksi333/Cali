using Cali.Core.Abstractions;
using Cali.Core.Models;

namespace Cali.Core.Services;

public sealed class SettingsService
{
    private readonly IKeyValueStore _kv;
    public SettingsService(IKeyValueStore kv) => _kv = kv;

    private const string K = "cali.";

    public AppSettings Settings => new()
    {
        SoundAlerts = _kv.GetBool(K + "soundAlerts", true),
        Haptics = _kv.GetBool(K + "haptics", true),
        AutoStartRest = _kv.GetBool(K + "autoStartRest", true),
        AutoProgression = _kv.GetBool(K + "autoProgression", false),
        Reminders = _kv.GetBool(K + "reminders", true),
        Units = _kv.GetString(K + "units", "metric"),
    };

    public void Save(AppSettings s)
    {
        _kv.SetBool(K + "soundAlerts", s.SoundAlerts);
        _kv.SetBool(K + "haptics", s.Haptics);
        _kv.SetBool(K + "autoStartRest", s.AutoStartRest);
        _kv.SetBool(K + "autoProgression", s.AutoProgression);
        _kv.SetBool(K + "reminders", s.Reminders);
        _kv.SetString(K + "units", s.Units);
    }

    public bool IsOnboardingComplete
    {
        get => _kv.GetBool(K + "onboarded", false);
        set => _kv.SetBool(K + "onboarded", value);
    }

    public int CurrentStreak { get => _kv.GetInt(K + "streak", 0); set => _kv.SetInt(K + "streak", value); }
    public int BestStreak { get => _kv.GetInt(K + "bestStreak", 0); set => _kv.SetInt(K + "bestStreak", value); }

    public Profile Profile
    {
        get => new()
        {
            Name = _kv.GetString(K + "p.name", "Athlete"),
            Goal = _kv.GetString(K + "p.goal", "Strength"),
            Level = _kv.GetString(K + "p.level", "Beginner"),
            BodyweightKg = _kv.GetInt(K + "p.bw", 0),
            MaxPullups = _kv.GetInt(K + "p.maxpu", 0),
        };
        set
        {
            _kv.SetString(K + "p.name", value.Name);
            _kv.SetString(K + "p.goal", value.Goal);
            _kv.SetString(K + "p.level", value.Level);
            _kv.SetInt(K + "p.bw", value.BodyweightKg);
            _kv.SetInt(K + "p.maxpu", value.MaxPullups);
        }
    }

    // Generic one-time flag helpers (used by SeedService).
    public bool IsKeySet(string key) => _kv.Contains(key);
    public void MarkKey(string key) => _kv.SetBool(key, true);
}
