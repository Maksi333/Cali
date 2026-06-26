using Cali.Core.Models;

namespace Cali.Core.Services;

/// <summary>Builds a <see cref="StatsContext"/> from local data once per evaluation. All date math is local-time.</summary>
public static class StatsContextBuilder
{
    // Skill is "unlocked" when its representative exercise has a PR. (l_sit/front_lever/planche have no exercise.)
    public static readonly IReadOnlyDictionary<string, string> SkillExercise = new Dictionary<string, string>
    {
        ["pullup"] = "pullup", ["dip"] = "dips", ["pistol_squat"] = "pistol", ["handstand"] = "handstand", ["muscle_up"] = "muscleup"
    };

    public static StatsContext Build(
        IReadOnlyList<WorkoutRecord> history,
        IReadOnlyList<PersonalRecord> prs,
        int customPlansCreated,
        IReadOnlyCollection<string> presetPlanNames,
        IReadOnlyDictionary<string, string> exerciseFamily,
        TimeZoneInfo tz,
        DateOnly today)
    {
        var locals = history.Select(r =>
        {
            var t = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(r.DateUtc, DateTimeKind.Utc), tz);
            return (r, date: DateOnly.FromDateTime(t), hour: t.Hour);
        }).ToList();

        var days = locals.Select(x => x.date).Distinct().OrderBy(d => d).ToList();
        var (current, best) = StreakCalculator.Compute(days, today);

        // Family rep volume + distinct exercises (from per-workout logs).
        var repsByFamily = new Dictionary<string, int>();
        var distinct = new HashSet<string>();
        foreach (var r in history)
            foreach (var it in r.Items())
            {
                if (!string.IsNullOrEmpty(it.ExerciseId)) distinct.Add(it.ExerciseId);
                var fam = !string.IsNullOrEmpty(it.Family) ? it.Family : (exerciseFamily.TryGetValue(it.ExerciseId, out var f) ? f : "");
                if (!string.IsNullOrEmpty(fam)) repsByFamily[fam] = repsByFamily.GetValueOrDefault(fam) + it.Reps;
            }

        // Best single set / hold per family, from PRs.
        var bestSet = new Dictionary<string, int>();
        var bestHold = new Dictionary<string, int>();
        foreach (var pr in prs)
        {
            var fam = exerciseFamily.TryGetValue(pr.ExerciseId, out var f) ? f : "";
            if (string.IsNullOrEmpty(fam)) continue;
            if (pr.MaxReps is int mr) bestSet[fam] = Math.Max(bestSet.GetValueOrDefault(fam), mr);
            if (pr.LongestHoldSec is int lh) bestHold[fam] = Math.Max(bestHold.GetValueOrDefault(fam), lh);
        }

        var unlockedSkills = new HashSet<string>();
        foreach (var (skill, exId) in SkillExercise)
            if (prs.Any(p => p.ExerciseId == exId && ((p.MaxReps ?? 0) >= 1 || (p.LongestHoldSec ?? 0) >= 1)))
                unlockedSkills.Add(skill);

        var completedPresets = history.Select(r => r.PlanName).Where(presetPlanNames.Contains).Distinct().Count();

        // Per-week bucketing (Monday-start).
        var byWeek = locals.GroupBy(x => MondayOf(x.date)).ToList();
        bool perfectWeek = byWeek.Any(g => g.Select(x => x.date.DayOfWeek).Distinct().Count() == 7);
        bool weekendBoth = byWeek.Any(g =>
        {
            var dows = g.Select(x => x.date.DayOfWeek).ToHashSet();
            return dows.Contains(DayOfWeek.Saturday) && dows.Contains(DayOfWeek.Sunday);
        });
        int maxMusclesInWeek = byWeek.Count == 0 ? 0 : byWeek.Max(g =>
            g.SelectMany(x => x.r.Items()).Select(it => it.Muscle).Where(m => !string.IsNullOrEmpty(m)).Distinct().Count());

        // Comeback gap = largest jump between consecutive workout days.
        int longestGap = 0;
        for (int i = 1; i < days.Count; i++) longestGap = Math.Max(longestGap, days[i].DayNumber - days[i - 1].DayNumber);

        return new StatsContext
        {
            TotalWorkouts = history.Count,
            CurrentStreakDays = current,
            BestStreakDays = best,
            CumulativeDurationMin = history.Sum(r => r.DurationSec) / 60,
            TotalReps = history.Sum(r => r.TotalReps),
            RepsByFamily = repsByFamily,
            BestSetRepsByFamily = bestSet,
            BestHoldSecondsByFamily = bestHold,
            DistinctExerciseIds = distinct,
            CustomPlansCreated = customPlansCreated,
            AllPredefinedCompleted = presetPlanNames.Count > 0 && completedPresets >= presetPlanNames.Count,
            CompletedModes = new HashSet<string>(),
            UnlockedSkillIds = unlockedSkills,
            LongestWorkoutMin = history.Count == 0 ? 0 : history.Max(r => r.DurationSec) / 60,
            HadFastWorkout = history.Any(r => r.DurationSec > 0 && r.DurationSec <= 600 && r.ExerciseCount >= 3),
            MaxWorkoutsInOneDay = locals.Count == 0 ? 0 : locals.GroupBy(x => x.date).Max(g => g.Count()),
            MaxWorkoutsInCalendarMonth = locals.Count == 0 ? 0 : locals.GroupBy(x => (x.date.Year, x.date.Month)).Max(g => g.Count()),
            HadPerfectWeek = perfectWeek,
            HadWeekendBothDays = weekendBoth,
            MaxMuscleGroupsInAWeek = maxMusclesInWeek,
            LongestComebackGapDays = longestGap,
            EarliestWorkoutStartHour = locals.Count == 0 ? 24 : locals.Min(x => x.hour),
            LatestWorkoutStartHour = locals.Count == 0 ? -1 : locals.Max(x => x.hour),
            WorkoutStartHours = locals.Select(x => x.hour).ToList(),
            MaxPrsInSingleWorkout = history.Count == 0 ? 0 : history.Max(r => r.PrCount),
            EffortRatingsLogged = history.Count,
        };
    }

    private static DateOnly MondayOf(DateOnly d)
    {
        int diff = ((int)d.DayOfWeek + 6) % 7; // Mon->0 … Sun->6
        return d.AddDays(-diff);
    }
}
