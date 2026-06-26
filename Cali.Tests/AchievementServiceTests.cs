using Cali.Core.Models;
using Cali.Core.Services;
using Cali.Tests.Fakes;
using Xunit;

public class AchievementEngineTests
{
    // ---------- StreakCalculator ----------
    [Fact]
    public void Streak_current_and_best()
    {
        var today = new DateOnly(2026, 6, 24);
        var (c, b) = StreakCalculator.Compute(
            new[] { new DateOnly(2026, 6, 22), new DateOnly(2026, 6, 23), new DateOnly(2026, 6, 24) }, today);
        Assert.Equal(3, c); Assert.Equal(3, b);

        var (c2, b2) = StreakCalculator.Compute(
            new[] { new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 11), new DateOnly(2026, 6, 12), new DateOnly(2026, 6, 20) }, today);
        Assert.Equal(0, c2); Assert.Equal(3, b2);

        Assert.Equal((0, 0), StreakCalculator.Compute(Array.Empty<DateOnly>(), today));
        Assert.Equal(1, StreakCalculator.Compute(new[] { new DateOnly(2026, 6, 23) }, today).current); // yesterday counts
    }

    // ---------- StatsContextBuilder ----------
    private static WorkoutRecord W(DateTime utc, int durSec, int reps, int exCount, int prCount, params WorkoutItemLog[] items)
    {
        var r = new WorkoutRecord { DateUtc = utc, DurationSec = durSec, TotalReps = reps, ExerciseCount = exCount, PrCount = prCount, PlanName = "P", Effort = Effort.JustRight };
        r.SetItems(items);
        return r;
    }
    private static WorkoutItemLog Item(string id, string fam, string muscle, int reps, int bestSet = 0, int hold = 0) =>
        new() { ExerciseId = id, Family = fam, Muscle = muscle, Reps = reps, BestSetReps = bestSet, HoldSec = hold };

    private static readonly Dictionary<string, string> Fam = new()
    { ["pushup"] = "pushup", ["squat"] = "squat", ["plank"] = "plank", ["handstand"] = "", ["pullup"] = "pullup" };

    [Fact]
    public void Builder_aggregates_volume_distinct_day_gap_hours()
    {
        var history = new List<WorkoutRecord>
        {
            W(new DateTime(2026,6,22,6,0,0,DateTimeKind.Utc), 1800, 40, 1, 1, Item("pushup","pushup","Chest",40,10)),
            W(new DateTime(2026,6,22,20,0,0,DateTimeKind.Utc), 600, 50, 3, 0, Item("squat","squat","Legs",50,10)),
            W(new DateTime(2026,6,24,9,0,0,DateTimeKind.Utc), 1200, 30, 2, 0, Item("pushup","pushup","Chest",30,10), Item("plank","plank","Core",0,0,45)),
        };
        var prs = new List<PersonalRecord>
        {
            new() { ExerciseId = "pushup", MaxReps = 12 },
            new() { ExerciseId = "handstand", LongestHoldSec = 15 },
        };
        var s = StatsContextBuilder.Build(history, prs, customPlansCreated: 0,
            presetPlanNames: new HashSet<string>(), exerciseFamily: Fam, TimeZoneInfo.Utc, new DateOnly(2026, 6, 24));

        Assert.Equal(3, s.TotalWorkouts);
        Assert.Equal(120, s.TotalReps);
        Assert.Equal(60, s.CumulativeDurationMin);
        Assert.Equal(70, s.FamilyReps("pushup"));   // 40 + 30
        Assert.Equal(50, s.FamilyReps("squat"));
        Assert.Equal(12, s.FamilyBestSet("pushup")); // from PR
        Assert.Equal(3, s.DistinctExerciseIds.Count);
        Assert.Equal(2, s.MaxWorkoutsInOneDay);      // two on the 22nd
        Assert.Equal(2, s.LongestComebackGapDays);   // 22 -> 24
        Assert.Equal(6, s.EarliestWorkoutStartHour);
        Assert.Equal(20, s.LatestWorkoutStartHour);
        Assert.True(s.HadFastWorkout);               // W2: 10min, 3 exercises
        Assert.Contains("handstand", s.UnlockedSkillIds);  // PR-derived skill
        Assert.Equal((1, 1), (s.CurrentStreakDays, s.BestStreakDays)); // 22 & 24 not consecutive
    }

    [Fact]
    public void Builder_detects_perfect_week_and_weekend()
    {
        var history = new List<WorkoutRecord>();
        for (int d = 15; d <= 21; d++) // Mon 2026-06-15 .. Sun 2026-06-21
            history.Add(W(new DateTime(2026, 6, d, 12, 0, 0, DateTimeKind.Utc), 1200, 10, 3, 0, Item("pushup", "pushup", "Chest", 10, 10)));

        var s = StatsContextBuilder.Build(history, new List<PersonalRecord>(), 0,
            new HashSet<string>(), Fam, TimeZoneInfo.Utc, new DateOnly(2026, 6, 24));

        Assert.True(s.HadPerfectWeek);
        Assert.True(s.HadWeekendBothDays);
        Assert.Equal(7, s.MaxWorkoutsInCalendarMonth); // 7 workouts in June
    }

    // ---------- CriteriaEvaluator ----------
    [Fact]
    public void Evaluator_covers_representative_types()
    {
        var s = new StatsContext
        {
            TotalWorkouts = 5,
            BestStreakDays = 7,
            RepsByFamily = new Dictionary<string, int> { ["pushup"] = 500 },
            EarliestWorkoutStartHour = 5,
            LatestWorkoutStartHour = 22,
            WorkoutStartHours = new[] { 12 },
            UnlockedSkillIds = new HashSet<string> { "handstand" },
            MaxPrsInSingleWorkout = 3,
        };
        bool P(Criteria c) => CriteriaEvaluator.Passes(c, s);

        Assert.True(P(new Criteria { Type = "total_workouts", Count = 5 }));
        Assert.False(P(new Criteria { Type = "total_workouts", Count = 6 }));
        Assert.True(P(new Criteria { Type = "streak_days", Days = 7 }));
        Assert.True(P(new Criteria { Type = "total_reps_in_family", Family = "pushup", Count = 500 }));
        Assert.True(P(new Criteria { Type = "workout_start_before_hour", Hour = 6 }));
        Assert.False(P(new Criteria { Type = "workout_start_before_hour", Hour = 5 }));
        Assert.True(P(new Criteria { Type = "workout_start_after_hour", Hour = 22 }));
        Assert.True(P(new Criteria { Type = "workout_start_between", StartHour = 11, EndHour = 14 }));
        Assert.True(P(new Criteria { Type = "skill_unlocked", SkillId = "handstand" }));
        Assert.False(P(new Criteria { Type = "skill_unlocked", SkillId = "muscle_up" }));
        Assert.False(P(new Criteria { Type = "all_modes_completed", Modes = new[] { "standard" } }));
        Assert.True(P(new Criteria { Type = "prs_in_single_workout", Count = 3 }));
    }

    [Fact]
    public void Catalog_has_expected_shape()
    {
        Assert.Equal(54, AchievementCatalog.All.Count);
        Assert.Equal(3, AchievementCatalog.All.Count(a => a.Hidden));               // secret achievements
        Assert.All(AchievementCatalog.All, a => Assert.Contains(a.Category, AchievementCatalog.Categories));
    }

    // ---------- AchievementService (integration) ----------
    [Fact]
    public async Task Sync_over_fresh_install_unlocks_nothing()
    {
        // BUG-004: a brand-new install (no history, no plans, no PRs) must unlock zero achievements.
        var path = Path.Combine(Path.GetTempPath(), $"cali-{Guid.NewGuid():N}.db3");
        var db = new Database(path);
        await db.InitAsync();
        try
        {
            var history = new HistoryRepository(db);
            var prs = new PrRepository(db);
            var plans = new PlanRepository(db);
            var exercises = new ExerciseRepository(new FileSeedDataProvider());
            await exercises.InitAsync();
            var svc = new AchievementService(db, history, prs, plans, exercises);

            var newly = await svc.SyncAsync(new DateTime(2026, 6, 25, 12, 0, 0, DateTimeKind.Utc), celebrate: false);

            Assert.Empty(newly);
            Assert.Empty(svc.PendingCelebrations);
            var status = await svc.GetStatusAsync();
            Assert.All(status, s => Assert.False(s.Unlocked));
        }
        finally { try { File.Delete(path); } catch { } }
    }

    [Fact]
    public async Task Sync_unlocks_once_and_status_reflects_state()
    {
        var path = Path.Combine(Path.GetTempPath(), $"cali-{Guid.NewGuid():N}.db3");
        var db = new Database(path);
        await db.InitAsync();
        try
        {
            var history = new HistoryRepository(db);
            var prs = new PrRepository(db);
            var plans = new PlanRepository(db);
            var exercises = new ExerciseRepository(new FileSeedDataProvider());
            await exercises.InitAsync();
            var svc = new AchievementService(db, history, prs, plans, exercises);

            var now = new DateTime(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc);
            var rec = W(now, 1200, 30, 3, 0, Item("pushup", "pushup", "Chest", 30, 10));
            await history.AddAsync(rec);

            var first = await svc.SyncAsync(now, celebrate: false);
            Assert.Contains("first_workout", first);
            Assert.Empty(await svc.SyncAsync(now, celebrate: true));   // idempotent
            Assert.Empty(svc.PendingCelebrations);

            // Create a custom plan -> first_plan unlocks (celebrated).
            await plans.SaveAsync(new Plan { Id = "u1", Name = "Mine", Preset = false, Difficulty = Difficulty.Beginner });
            var newly = await svc.SyncAsync(now, celebrate: true);
            Assert.Contains("first_plan", newly);
            Assert.Contains("first_plan", svc.PendingCelebrations);

            var status = await svc.GetStatusAsync();
            Assert.Equal(54, status.Count);
            Assert.True(status.Single(s => s.Id == "first_workout").Unlocked);
            Assert.True(status.Single(s => s.Id == "honest_effort").Hidden);
            Assert.False(status.Single(s => s.Id == "workouts_500").Unlocked);
            Assert.Equal(1, status.Single(s => s.Id == "first_workout").Cur);
        }
        finally { try { File.Delete(path); } catch { } }
    }
}
