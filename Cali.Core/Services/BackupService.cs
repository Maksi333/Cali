using System.Text.Json;
using System.Text.Json.Serialization;
using Cali.Core.Models;

namespace Cali.Core.Services;

/// <summary>Serializes user plans + history + profile/settings to JSON and merges them back (local-first backup).</summary>
public sealed class BackupService
{
    private readonly PlanRepository _plans;
    private readonly HistoryRepository _history;
    private readonly SettingsService _settings;

    private static readonly JsonSerializerOptions Opts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public BackupService(PlanRepository plans, HistoryRepository history, SettingsService settings)
    {
        _plans = plans;
        _history = history;
        _settings = settings;
    }

    public async Task<string> ExportJsonAsync()
    {
        var data = new BackupData
        {
            Plans = await _plans.GetUserPlansAsync(),
            History = await _history.GetRecentAsync(int.MaxValue),
            Profile = _settings.Profile,
            Settings = _settings.Settings,
            CurrentStreak = _settings.CurrentStreak,
            BestStreak = _settings.BestStreak
        };
        return JsonSerializer.Serialize(data, Opts);
    }

    public async Task ImportJsonAsync(string json)
    {
        var data = JsonSerializer.Deserialize<BackupData>(json, Opts);
        if (data is null) return;

        foreach (var p in data.Plans)
        {
            p.Preset = false;
            if (string.IsNullOrEmpty(p.Id)) p.Id = Guid.NewGuid().ToString("N");
            await _plans.SaveAsync(p);
        }
        foreach (var h in data.History)
        {
            h.Id = 0;
            await _history.AddAsync(h);
        }
        if (data.Profile is not null) _settings.Profile = data.Profile;
        if (data.Settings is not null) _settings.Save(data.Settings);
        _settings.CurrentStreak = data.CurrentStreak;
        _settings.BestStreak = data.BestStreak;
    }
}

public sealed class BackupData
{
    public List<Plan> Plans { get; set; } = new();
    public List<WorkoutRecord> History { get; set; } = new();
    public Profile? Profile { get; set; }
    public AppSettings? Settings { get; set; }
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }
}
