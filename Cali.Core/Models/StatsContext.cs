namespace Cali.Core.Models;

/// <summary>Precomputed aggregates the criteria engine reads (O(1) per achievement).</summary>
public sealed class StatsContext
{
    public int TotalWorkouts { get; init; }
    public int CurrentStreakDays { get; init; }
    public int BestStreakDays { get; init; }
    public int CumulativeDurationMin { get; init; }
    public int TotalReps { get; init; }
    public IReadOnlyDictionary<string, int> RepsByFamily { get; init; } = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> BestSetRepsByFamily { get; init; } = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> BestHoldSecondsByFamily { get; init; } = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> BestRepsByExercise { get; init; } = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> BestHoldByExercise { get; init; } = new Dictionary<string, int>();
    public IReadOnlySet<string> DistinctExerciseIds { get; init; } = new HashSet<string>();
    public int CustomPlansCreated { get; init; }
    public bool AllPredefinedCompleted { get; init; }
    public IReadOnlySet<string> CompletedModes { get; init; } = new HashSet<string>();
    public IReadOnlySet<string> UnlockedSkillIds { get; init; } = new HashSet<string>();
    public int LongestWorkoutMin { get; init; }
    public bool HadFastWorkout { get; init; }
    public int MaxWorkoutsInOneDay { get; init; }
    public int MaxWorkoutsInCalendarMonth { get; init; }
    public bool HadPerfectWeek { get; init; }
    public bool HadWeekendBothDays { get; init; }
    public int MaxMuscleGroupsInAWeek { get; init; }
    public int LongestComebackGapDays { get; init; }
    public int EarliestWorkoutStartHour { get; init; } = 24;
    public int LatestWorkoutStartHour { get; init; } = -1;
    public IReadOnlyList<int> WorkoutStartHours { get; init; } = Array.Empty<int>();
    public int MaxPrsInSingleWorkout { get; init; }
    public int EffortRatingsLogged { get; init; }

    public int FamilyReps(string f) => RepsByFamily.TryGetValue(f, out var v) ? v : 0;
    public int FamilyBestSet(string f) => BestSetRepsByFamily.TryGetValue(f, out var v) ? v : 0;
    public int FamilyBestHold(string f) => BestHoldSecondsByFamily.TryGetValue(f, out var v) ? v : 0;
    public int ExerciseBestSet(string id) => BestRepsByExercise.TryGetValue(id, out var v) ? v : 0;
    public int ExerciseBestHold(string id) => BestHoldByExercise.TryGetValue(id, out var v) ? v : 0;
}
