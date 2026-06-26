using Cali.Core.Models;

namespace Cali.Core.Services;

/// <summary>Evaluates a typed <see cref="Criteria"/> against a <see cref="StatsContext"/>.</summary>
public static class CriteriaEvaluator
{
    public static bool Passes(Criteria c, StatsContext s) => c.Type switch
    {
        "total_workouts" => s.TotalWorkouts >= c.Count,
        "streak_days" => Math.Max(s.CurrentStreakDays, s.BestStreakDays) >= c.Days,
        "perfect_calendar_week" => s.HadPerfectWeek,
        "workouts_in_calendar_month" => s.MaxWorkoutsInCalendarMonth >= c.Count,
        "weekend_both_days" => s.HadWeekendBothDays,
        "comeback_gap_days" => s.LongestComebackGapDays >= c.Days,
        "total_reps" => s.TotalReps >= c.Count,
        "total_reps_in_family" => s.FamilyReps(c.Family) >= c.Count,
        "single_set_reps" => s.FamilyBestSet(c.Family) >= c.Count,
        "single_hold_seconds" => s.FamilyBestHold(c.Family) >= c.Seconds,
        "single_workout_duration_min" => s.LongestWorkoutMin >= c.Minutes,
        "single_workout_duration_max" => s.HadFastWorkout,
        "cumulative_duration_minutes" => s.CumulativeDurationMin >= c.Minutes,
        "workouts_in_single_day" => s.MaxWorkoutsInOneDay >= c.Count,
        "workout_start_before_hour" => s.EarliestWorkoutStartHour < c.Hour,
        "workout_start_after_hour" => s.LatestWorkoutStartHour >= c.Hour,
        "workout_start_between" => s.WorkoutStartHours.Any(h => h >= c.StartHour && h < c.EndHour),
        "distinct_exercises_used" => s.DistinctExerciseIds.Count >= c.Count,
        "distinct_muscle_groups_in_week" => s.MaxMuscleGroupsInAWeek >= c.Count,
        "custom_plans_created" => s.CustomPlansCreated >= c.Count,
        "predefined_plans_all_completed" => s.AllPredefinedCompleted,
        "all_modes_completed" => c.Modes.Length > 0 && c.Modes.All(m => s.CompletedModes.Contains(m)),
        "skill_unlocked" => s.UnlockedSkillIds.Contains(c.SkillId),
        "prs_in_single_workout" => s.MaxPrsInSingleWorkout >= c.Count,
        "effort_ratings_logged" => s.EffortRatingsLogged >= c.Count,
        _ => false
    };

    /// <summary>Current/target for UI progress hints. Boolean criteria report (0|1, 1).</summary>
    public static (int cur, int target) Progress(Criteria c, StatsContext s) => c.Type switch
    {
        "total_workouts" => (s.TotalWorkouts, c.Count),
        "streak_days" => (Math.Max(s.CurrentStreakDays, s.BestStreakDays), c.Days),
        "workouts_in_calendar_month" => (s.MaxWorkoutsInCalendarMonth, c.Count),
        "comeback_gap_days" => (s.LongestComebackGapDays, c.Days),
        "total_reps" => (s.TotalReps, c.Count),
        "total_reps_in_family" => (s.FamilyReps(c.Family), c.Count),
        "single_set_reps" => (s.FamilyBestSet(c.Family), c.Count),
        "single_hold_seconds" => (s.FamilyBestHold(c.Family), c.Seconds),
        "single_workout_duration_min" => (s.LongestWorkoutMin, c.Minutes),
        "cumulative_duration_minutes" => (s.CumulativeDurationMin, c.Minutes),
        "workouts_in_single_day" => (s.MaxWorkoutsInOneDay, c.Count),
        "distinct_exercises_used" => (s.DistinctExerciseIds.Count, c.Count),
        "distinct_muscle_groups_in_week" => (s.MaxMuscleGroupsInAWeek, c.Count),
        "custom_plans_created" => (s.CustomPlansCreated, c.Count),
        "prs_in_single_workout" => (s.MaxPrsInSingleWorkout, c.Count),
        "effort_ratings_logged" => (s.EffortRatingsLogged, c.Count),
        _ => (Passes(c, s) ? 1 : 0, 1)
    };
}
