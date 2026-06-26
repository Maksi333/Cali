using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cali.Core.Models;
using Cali.Core.Services;
using Microsoft.Maui.Graphics;

namespace Cali.ViewModels;

public partial class SummaryViewModel : ObservableObject
{
    private readonly WorkoutSessionService _session;
    private readonly HistoryRepository _history;
    private readonly PrRepository _prs;
    private readonly AchievementService _achievements;
    private string _planName = "";
    private readonly List<(string id, int reps, int holdSec, bool isHold)> _prCandidates = new();

    public SummaryViewModel(WorkoutSessionService session, HistoryRepository history, PrRepository prs, AchievementService achievements)
    {
        _session = session;
        _history = history;
        _prs = prs;
        _achievements = achievements;
    }

    [ObservableProperty] private string planLine = "";
    [ObservableProperty] private string durationText = "0:00";
    [ObservableProperty] private int exerciseCount;
    [ObservableProperty] private int totalSets;
    [ObservableProperty] private int totalReps;
    [ObservableProperty] private bool hasPr;
    [ObservableProperty] private string prText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EasyBg))]
    [NotifyPropertyChangedFor(nameof(JustBg))]
    [NotifyPropertyChangedFor(nameof(HardBg))]
    [NotifyPropertyChangedFor(nameof(EasyText))]
    [NotifyPropertyChangedFor(nameof(JustText))]
    [NotifyPropertyChangedFor(nameof(HardText))]
    private int effortIndex = 1;

    public Color EasyBg => EffortIndex == 0 ? Tokens.Success : Tokens.Surface2;
    public Color JustBg => EffortIndex == 1 ? Tokens.Accent : Tokens.Surface2;
    public Color HardBg => EffortIndex == 2 ? Tokens.Danger : Tokens.Surface2;
    public Color EasyText => EffortIndex == 0 ? Tokens.AccentOn : Tokens.TextMuted;
    public Color JustText => EffortIndex == 1 ? Tokens.AccentOn : Tokens.TextMuted;
    public Color HardText => EffortIndex == 2 ? Tokens.AccentOn : Tokens.TextMuted;

    public event Action? CloseRequested;

    public async Task PrepareAsync(string planName)
    {
        _planName = planName;
        ExerciseCount = _session.ExerciseCount;
        TotalSets = _session.TotalSetsDone;
        TotalReps = _session.TotalRepsDone;
        DurationText = Fmt(_session.SessionSeconds);
        PlanLine = $"{planName} · {ExerciseCount} exercises";
        await DetectPrsAsync();
    }

    private async Task DetectPrsAsync()
    {
        _prCandidates.Clear();
        string? best = null;
        for (int i = 0; i < _session.Items.Count; i++)
        {
            if (_session.DoneSetsAt(i) <= 0) continue;
            var item = _session.Items[i];
            int bestSet = _session.BestSetAt(i); // best ACTUAL logged value this session
            if (bestSet <= 0) continue;
            if (item.Exercise.Type == ExerciseType.Reps)
            {
                var pr = await _prs.GetAsync(item.Exercise.Id);
                if (pr?.MaxReps is null || bestSet > pr.MaxReps)
                {
                    _prCandidates.Add((item.Exercise.Id, bestSet, 0, false));
                    best ??= $"{item.Exercise.Name} · {bestSet} reps";
                }
            }
            else // Hold
            {
                var pr = await _prs.GetAsync(item.Exercise.Id);
                if (pr?.LongestHoldSec is null || bestSet > pr.LongestHoldSec)
                {
                    _prCandidates.Add((item.Exercise.Id, 0, bestSet, true));
                    best ??= $"{item.Exercise.Name} · {bestSet}s hold";
                }
            }
        }
        HasPr = best is not null;
        PrText = best ?? "";
    }

    [RelayCommand]
    private void SetEffort(string index)
    {
        if (int.TryParse(index, out var i)) EffortIndex = i;
    }

    [RelayCommand]
    private async Task SaveToHistory()
    {
        var now = DateTime.UtcNow;
        foreach (var c in _prCandidates)
        {
            if (c.isHold) await _prs.TryBeatHoldAsync(c.id, c.holdSec, now);
            else await _prs.TryBeatRepsAsync(c.id, c.reps, now);
        }

        // Per-exercise log (powers volume-by-family / distinct-exercises / muscle-week achievements).
        var items = new List<WorkoutItemLog>();
        for (int i = 0; i < _session.Items.Count; i++)
        {
            var done = _session.DoneSetsAt(i);
            if (done <= 0) continue;
            var it = _session.Items[i];
            bool isHold = it.Exercise.Type == ExerciseType.Hold;
            items.Add(new WorkoutItemLog
            {
                ExerciseId = it.Exercise.Id,
                Family = it.Exercise.Family,
                Muscle = it.Exercise.Primary,
                Sets = done,
                Reps = isHold ? 0 : _session.LoggedSumAt(i),       // actual total reps logged
                BestSetReps = isHold ? 0 : _session.BestSetAt(i),  // best actual set
                HoldSec = isHold ? _session.BestSetAt(i) : 0       // best actual hold (seconds)
            });
        }

        var record = new WorkoutRecord
        {
            DateUtc = now,
            PlanName = _planName,
            DurationSec = _session.SessionSeconds,
            ExerciseCount = ExerciseCount,
            TotalSets = TotalSets,
            TotalReps = TotalReps,
            Effort = (Effort)EffortIndex,
            PrCount = _prCandidates.Count
        };
        record.SetItems(items);
        await _history.AddAsync(record);

        // Newly-earned achievements -> queued summary toasts (animated before navigating).
        var newlyIds = await _achievements.SyncAsync(now, celebrate: false);
        NewlyUnlocked = newlyIds
            .Select(id => AchievementCatalog.All.First(a => a.Id == id))
            .Select(a => new ToastItem(a.Emoji, a.Name, TierColors.Of(a.Tier)))
            .ToList();

        CloseRequested?.Invoke();
    }

    public List<ToastItem> NewlyUnlocked { get; private set; } = new();

    private static string Fmt(int s) => $"{s / 60}:{s % 60:00}";
}

public sealed record ToastItem(string Emoji, string Name, Microsoft.Maui.Graphics.Color TierColor);
