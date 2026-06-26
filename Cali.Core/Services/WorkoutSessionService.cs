using CommunityToolkit.Mvvm.ComponentModel;
using Cali.Core.Abstractions;
using Cali.Core.Models;

namespace Cali.Core.Services;

/// <summary>
/// The hero. Owns all live workout state and the single 1-second tick that drives both the
/// count-up session timer and the count-down rest timer. UI binds directly to this.
/// </summary>
public sealed class WorkoutSessionService : ObservableObject
{
    private readonly ITicker _ticker;
    private readonly ExerciseRepository _exercises;
    private readonly SettingsService _settings;

    private List<WorkoutItem> _items = new();
    // Per-exercise log of the ACTUAL value (reps, or hold-seconds) the user did each completed set.
    private List<int>[] _setLog = Array.Empty<List<int>>();

    public WorkoutSessionService(ITicker ticker, ExerciseRepository exercises, SettingsService settings)
    {
        _ticker = ticker;
        _exercises = exercises;
        _settings = settings;
        _ticker.Tick += OnTick;
    }

    // ---- Backing state with targeted notifications ----
    private bool _running;
    public bool Running { get => _running; private set => SetProperty(ref _running, value); }

    private int _sessionSeconds;
    public int SessionSeconds
    {
        get => _sessionSeconds;
        private set { if (SetProperty(ref _sessionSeconds, value)) OnPropertyChanged(nameof(SessionDisplay)); }
    }

    private int _curIdx;
    public int CurIdx
    {
        get => _curIdx;
        private set { if (SetProperty(ref _curIdx, value)) OnPropertyChanged(string.Empty); }
    }

    private bool _restActive;
    public bool RestActive { get => _restActive; private set => SetProperty(ref _restActive, value); }

    private int _restSeconds;
    public int RestSeconds
    {
        get => _restSeconds;
        private set { if (SetProperty(ref _restSeconds, value)) { OnPropertyChanged(nameof(RestDisplay)); OnPropertyChanged(nameof(RestFraction)); } }
    }

    private int _restTotal;
    public int RestTotal
    {
        get => _restTotal;
        private set { if (SetProperty(ref _restTotal, value)) OnPropertyChanged(nameof(RestFraction)); }
    }

    private int _pendingValue;
    /// <summary>The actual value (reps, or hold-seconds) the user logs for the CURRENT set; defaults to the target.</summary>
    public int PendingValue { get => _pendingValue; private set => SetProperty(ref _pendingValue, value); }

    // ---- Derived ----
    public IReadOnlyList<WorkoutItem> Items => _items;
    public int ExerciseCount => _items.Count;
    public WorkoutItem? Current => CurIdx >= 0 && CurIdx < _items.Count ? _items[CurIdx] : null;
    public WorkoutItem? Next => CurIdx + 1 < _items.Count ? _items[CurIdx + 1] : null;
    public string? NextName => Next?.Exercise.Name;
    public int TotalSetsForCurrent => Current?.Sets ?? 0;
    public int DoneSetsForCurrent => CurIdx >= 0 && CurIdx < _setLog.Length ? _setLog[CurIdx].Count : 0;
    public bool CurrentIsHold => Current?.Exercise.Type == ExerciseType.Hold;
    public int TargetValue => CurrentIsHold ? Current?.HoldSec ?? 0 : Current?.Reps ?? 0;
    public string TargetUnitLabel
    {
        get
        {
            var setNo = Math.Min(DoneSetsForCurrent + 1, Math.Max(TotalSetsForCurrent, 1));
            return CurrentIsHold
                ? $"SEC HOLD · SET {setNo} OF {TotalSetsForCurrent}"
                : $"REPS · SET {setNo} OF {TotalSetsForCurrent}";
        }
    }
    public string SessionDisplay => Fmt(SessionSeconds);
    public string RestDisplay => Fmt(RestSeconds);
    public double RestFraction => RestTotal <= 0 ? 0 : Math.Clamp((double)RestSeconds / RestTotal, 0, 1);
    public string PositionLabel => ExerciseCount == 0 ? "0/0" : $"{Math.Min(CurIdx + 1, ExerciseCount)}/{ExerciseCount}";
    public double Progress => ExerciseCount == 0
        ? 0
        : (CurIdx + (TotalSetsForCurrent == 0 ? 0 : (double)DoneSetsForCurrent / TotalSetsForCurrent)) / ExerciseCount;

    // ---- End-of-workout totals (read by the Summary) ----
    public int DoneSetsAt(int index) => index >= 0 && index < _setLog.Length ? _setLog[index].Count : 0;
    public int TotalSetsDone => _items.Count == 0 ? 0 : _setLog.Take(_items.Count).Sum(l => l.Count);
    /// <summary>Best single logged set (reps or hold-seconds) for an exercise — used for PR detection.</summary>
    public int BestSetAt(int index) => index >= 0 && index < _setLog.Length && _setLog[index].Count > 0 ? _setLog[index].Max() : 0;
    /// <summary>Sum of all logged values for an exercise (= actual total reps for rep exercises).</summary>
    public int LoggedSumAt(int index) => index >= 0 && index < _setLog.Length ? _setLog[index].Sum() : 0;
    public int TotalRepsDone
    {
        get
        {
            int total = 0;
            for (int i = 0; i < _items.Count; i++)
                if (_items[i].Exercise.Type == ExerciseType.Reps)
                    total += _setLog[i].Sum();
            return total;
        }
    }

    private static string Fmt(int s) => $"{s / 60}:{s % 60:00}";

    // ---- Events ----
    public event Action? RestElapsed;
    public event Action? Finished;

    // ---- Commands ----
    public void Start(Plan plan)
    {
        _items = plan.Exercises
            .Select(pe => new { pe, ex = _exercises.Get(pe.ExerciseId) })
            .Where(x => x.ex is not null)
            .Select(x => new WorkoutItem
            {
                Exercise = x.ex!,
                Sets = x.pe.Sets,
                Reps = x.pe.Reps,
                HoldSec = x.pe.HoldSec,
                RestSec = x.pe.RestSec
            })
            .ToList();
        _setLog = new List<int>[_items.Count];
        for (int i = 0; i < _setLog.Length; i++) _setLog[i] = new List<int>();
        _sessionSeconds = 0;
        _curIdx = 0;
        _restActive = false;
        _restSeconds = 0;
        _restTotal = 0;
        Running = true;
        ResetPending();
        OnPropertyChanged(string.Empty);
        _ticker.Start();
    }

    /// <summary>
    /// Advances the workout state machine. Completing a set:
    ///  • mid-exercise            → record the set, auto-start rest (if enabled); next call fills the next set;
    ///  • last set of an exercise → rest for the just-finished exercise (if enabled), then advance to the next exercise;
    ///  • last set of the last exercise → end the workout (→ Summary records exactly one session).
    /// </summary>
    public void CompleteSet()
    {
        if (Current is null) return;

        // Log the ACTUAL value (PendingValue, defaults to target) for the set just finished
        // (clamped to Sets so a double-tap can't over-record).
        if (_setLog[CurIdx].Count < TotalSetsForCurrent)
            _setLog[CurIdx].Add(PendingValue);

        bool exerciseDone = _setLog[CurIdx].Count >= TotalSetsForCurrent;
        bool lastExercise = CurIdx + 1 >= _items.Count;

        // Last set of the last exercise → finish (no trailing rest). Summary writes the single record.
        if (exerciseDone && lastExercise)
        {
            OnPropertyChanged(string.Empty);
            End();
            return;
        }

        // Last set of this exercise (more remain) → rest for the finished exercise, then advance.
        if (exerciseDone)
        {
            if (_settings.Settings.AutoStartRest)
                StartRest();   // reads the just-finished exercise's RestSec (CurIdx not yet advanced)
            CurIdx++;          // CurIdx setter broadcasts → title/Next/target/dots/position all refresh
            ResetPending();    // pending = next exercise's target
            return;
        }

        // Mid-exercise → rest between sets; the next CompleteSet fills the next dot.
        ResetPending();        // pending = same exercise's target for the next set
        OnPropertyChanged(string.Empty);
        if (_settings.Settings.AutoStartRest)
            StartRest();
    }

    // ---- Per-set actual value the user is about to log (defaults to the target) ----
    public void IncPending() => PendingValue = Math.Min(PendingValue + (CurrentIsHold ? 5 : 1), 999);
    public void DecPending() => PendingValue = Math.Max(PendingValue - (CurrentIsHold ? 5 : 1), 0);
    private void ResetPending() => PendingValue = TargetValue;

    public void StartRest(int? total = null)
    {
        RestTotal = total ?? Current?.RestSec ?? 60;
        RestSeconds = RestTotal;
        RestActive = true;
    }

    public void AddRest(int sec = 30) => RestSeconds += sec;

    public void SkipRest() => RestActive = false;

    public void SetRestPreset(int sec)
    {
        RestTotal = sec;
        RestSeconds = sec;
    }

    public void GoNext()
    {
        RestActive = false;
        if (CurIdx + 1 >= _items.Count)
        {
            End(); // past the last exercise → end cleanly (stops the ticker, then raises Finished)
            return;
        }
        CurIdx++;
        ResetPending();
    }

    public void GoPrev()
    {
        RestActive = false;
        if (CurIdx > 0) { CurIdx--; ResetPending(); }
    }

    public void End()
    {
        if (!Running) return; // idempotent: a workout ends — and records — exactly once
        RestActive = false;
        Running = false;
        _ticker.Stop();
        Finished?.Invoke();
    }

    private void OnTick()
    {
        if (!Running) return;
        SessionSeconds++;
        if (RestActive)
        {
            RestSeconds--;
            if (RestSeconds <= 0)
            {
                RestSeconds = 0;
                RestActive = false;
                RestElapsed?.Invoke();
            }
        }
    }
}
