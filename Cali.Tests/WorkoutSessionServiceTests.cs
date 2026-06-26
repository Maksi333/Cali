using Cali.Core.Models;
using Cali.Core.Services;
using Cali.Tests.Fakes;
using Xunit;

public class WorkoutSessionServiceTests
{
    private static async Task<(WorkoutSessionService svc, FakeTicker ticker)> MakeAsync(bool autoRest = true)
    {
        var exRepo = new ExerciseRepository(new FileSeedDataProvider());
        await exRepo.InitAsync();
        var settings = new SettingsService(new InMemoryKeyValueStore());
        var s = settings.Settings; s.AutoStartRest = autoRest; settings.Save(s);
        var ticker = new FakeTicker();
        return (new WorkoutSessionService(ticker, exRepo, settings), ticker);
    }

    private static Plan PushDay()
    {
        var seed = SeedData.Parse(File.ReadAllText(FileSeedDataProvider.SeedPath));
        var sp = seed.PredefinedPlans.Single(p => p.Id == "push-day");
        return new Plan
        {
            Id = sp.Id, Name = sp.Name, Preset = true,
            Difficulty = Enum.Parse<Difficulty>(sp.Difficulty, true),
            EstMinutes = sp.EstMinutes, Muscles = sp.Muscles,
            Exercises = sp.Exercises.Select((e, i) => new PlanExercise
            {
                ExerciseId = e.ExerciseId, Order = i, Sets = e.Sets,
                Reps = e.Reps, HoldSec = e.HoldSec, RestSec = e.RestSec
            }).ToList()
        };
    }

    // Builds a minimal custom plan from exercise ids that exist in the seed (e.g. "pushup", "squat", "plank").
    private static Plan CustomPlan(params (string exId, int sets, int? reps, int? hold, int rest)[] items) =>
        new()
        {
            Id = "custom", Name = "Custom", Preset = false, Difficulty = Difficulty.Beginner,
            EstMinutes = 10, Muscles = "",
            Exercises = items.Select((e, i) => new PlanExercise
            {
                ExerciseId = e.exId, Order = i, Sets = e.sets, Reps = e.reps, HoldSec = e.hold, RestSec = e.rest
            }).ToList()
        };

    [Fact]
    public async Task Session_counts_up_each_tick()
    {
        var (svc, ticker) = await MakeAsync();
        svc.Start(PushDay());
        ticker.Fire(3);
        Assert.Equal(3, svc.SessionSeconds);
        Assert.Equal("0:03", svc.SessionDisplay);
    }

    [Fact]
    public async Task Resolves_items_from_plan()
    {
        var (svc, _) = await MakeAsync();
        svc.Start(PushDay());
        Assert.Equal(6, svc.ExerciseCount);
        Assert.Equal("Push-ups", svc.Current!.Exercise.Name);
        Assert.Equal("Pike Push-ups", svc.Next!.Exercise.Name);
    }

    [Fact]
    public async Task CompleteSet_fills_dot_and_autostarts_rest()
    {
        var (svc, _) = await MakeAsync(autoRest: true);
        svc.Start(PushDay());
        svc.CompleteSet();
        Assert.Equal(1, svc.DoneSetsForCurrent);
        Assert.True(svc.RestActive);
        Assert.Equal(60, svc.RestTotal); // push-up restSec
        Assert.Equal(60, svc.RestSeconds);
    }

    [Fact]
    public async Task CompleteSet_no_autostart_when_setting_off()
    {
        var (svc, _) = await MakeAsync(autoRest: false);
        svc.Start(PushDay());
        svc.CompleteSet();
        Assert.Equal(1, svc.DoneSetsForCurrent);
        Assert.False(svc.RestActive);
    }

    [Fact]
    public async Task Rest_counts_down_and_fires_elapsed()
    {
        var (svc, ticker) = await MakeAsync();
        svc.Start(PushDay());
        int elapsed = 0;
        svc.RestElapsed += () => elapsed++;
        svc.StartRest(3);
        ticker.Fire(3);
        Assert.False(svc.RestActive);
        Assert.Equal(0, svc.RestSeconds);
        Assert.Equal(1, elapsed);
        Assert.Equal(3, svc.SessionSeconds); // session keeps counting during rest
    }

    [Fact]
    public async Task AddRest_and_presets()
    {
        var (svc, _) = await MakeAsync();
        svc.Start(PushDay());
        svc.StartRest(30);
        svc.AddRest();
        Assert.Equal(60, svc.RestSeconds);
        svc.SetRestPreset(90);
        Assert.Equal(90, svc.RestTotal);
        Assert.Equal(90, svc.RestSeconds);
    }

    [Fact]
    public async Task Skip_cancels_rest()
    {
        var (svc, _) = await MakeAsync();
        svc.Start(PushDay());
        svc.StartRest(60);
        svc.SkipRest();
        Assert.False(svc.RestActive);
    }

    [Fact]
    public async Task Next_and_prev_move_index_and_cancel_rest()
    {
        var (svc, _) = await MakeAsync();
        svc.Start(PushDay());
        svc.StartRest(60);
        svc.GoNext();
        Assert.Equal("Pike Push-ups", svc.Current!.Exercise.Name);
        Assert.False(svc.RestActive);
        svc.GoPrev();
        Assert.Equal("Push-ups", svc.Current!.Exercise.Name);
    }

    [Fact]
    public async Task Next_past_last_raises_finished()
    {
        var (svc, _) = await MakeAsync();
        svc.Start(PushDay());
        bool finished = false;
        svc.Finished += () => finished = true;
        for (int i = 0; i < svc.ExerciseCount - 1; i++) svc.GoNext(); // to last
        svc.GoNext(); // past last
        Assert.True(finished);
    }

    [Fact]
    public async Task Totals_after_completing_sets()
    {
        var (svc, _) = await MakeAsync(autoRest: false);
        svc.Start(PushDay());
        svc.CompleteSet(); svc.CompleteSet(); // 2 sets of Push-ups (12 reps each)
        svc.GoNext();
        svc.CompleteSet();                    // 1 set of Pike Push-ups (10 reps)
        Assert.Equal(3, svc.TotalSetsDone);
        Assert.Equal(2 * 12 + 1 * 10, svc.TotalRepsDone);
    }

    [Fact]
    public async Task Progress_formula()
    {
        var (svc, _) = await MakeAsync(autoRest: false);
        svc.Start(PushDay());
        svc.GoNext(); // CurIdx = 1 (pike, 3 sets)
        svc.CompleteSet(); // 1 of 3 done
        Assert.Equal((1 + 1.0 / 3) / 6, svc.Progress, 5);
    }

    // ---- BUG-003: CompleteSet state machine (set → rest → next set → next exercise → end) ----

    [Fact]
    public async Task CompleteSet_lastSetOfExercise_advances_to_next_exercise()
    {
        var (svc, _) = await MakeAsync(autoRest: false);
        svc.Start(PushDay());           // pushups have 4 sets
        for (int i = 0; i < 4; i++) svc.CompleteSet();
        Assert.Equal(1, svc.CurIdx);
        Assert.Equal("Pike Push-ups", svc.Current!.Exercise.Name);
        Assert.Equal(0, svc.DoneSetsForCurrent);   // fresh exercise
        Assert.Equal("2/6", svc.PositionLabel);
        Assert.True(svc.Running);
        Assert.False(svc.RestActive);              // autoRest off → no overlay
    }

    [Fact]
    public async Task CompleteSet_lastSetOfExercise_autostarts_rest_then_advances()
    {
        var (svc, _) = await MakeAsync(autoRest: true);
        svc.Start(PushDay());
        for (int i = 0; i < 4; i++) svc.CompleteSet();  // finish pushups
        Assert.Equal(1, svc.CurIdx);                    // advanced underneath the rest overlay
        Assert.True(svc.RestActive);
        Assert.Equal(60, svc.RestTotal);                // the just-finished pushup's restSec
    }

    [Fact]
    public async Task CompleteSet_lastSetOfLastExercise_ends_workout_exactly_once()
    {
        var (svc, _) = await MakeAsync(autoRest: false);
        svc.Start(PushDay());
        int finished = 0;
        svc.Finished += () => finished++;
        int guard = 0;
        while (svc.Running && guard++ < 500) svc.CompleteSet();
        Assert.False(svc.Running);
        Assert.Equal(1, finished);
        Assert.False(svc.RestActive);
        Assert.Equal(4 + 3 + 3 + 3 + 3 + 3, svc.TotalSetsDone); // every set of push-day
    }

    [Fact]
    public async Task CompleteSet_finalSet_does_not_leave_rest_running()
    {
        var (svc, _) = await MakeAsync(autoRest: true);
        svc.Start(PushDay());
        int guard = 0;
        while (svc.Running && guard++ < 500) svc.CompleteSet();
        Assert.False(svc.Running);
        Assert.False(svc.RestActive); // ending dismisses any rest overlay
    }

    [Fact]
    public async Task CompleteSet_singleSetExercises_advance_then_end()
    {
        var (svc, _) = await MakeAsync(autoRest: false);
        svc.Start(CustomPlan(("pushup", 1, 10, null, 30), ("squat", 1, 12, null, 30)));
        int finished = 0;
        svc.Finished += () => finished++;
        svc.CompleteSet();                          // last set of pushup → advance
        Assert.Equal(1, svc.CurIdx);
        Assert.Equal("Squats", svc.Current!.Exercise.Name);
        svc.CompleteSet();                          // last set of last exercise → end
        Assert.False(svc.Running);
        Assert.Equal(1, finished);
    }

    [Fact]
    public async Task End_is_idempotent_records_exactly_one_finish()
    {
        // Guards the "exactly one session" criterion against re-tapping End / Complete / Next after the workout ends.
        var (svc, _) = await MakeAsync(autoRest: false);
        svc.Start(CustomPlan(("pushup", 1, 10, null, 30)));   // single set, single exercise
        int finished = 0;
        svc.Finished += () => finished++;

        svc.CompleteSet();          // final set of final exercise -> ends
        Assert.False(svc.Running);
        Assert.Equal(1, finished);

        svc.End();                  // re-tap End after it ended
        svc.CompleteSet();          // re-tap Complete after it ended
        svc.GoNext();               // re-tap Next after it ended
        Assert.Equal(1, finished);  // still exactly one
    }

    // ---- Per-set actual-rep logging ----

    [Fact]
    public async Task PendingValue_defaults_to_target()
    {
        var (svc, _) = await MakeAsync(autoRest: false);
        svc.Start(PushDay());                 // first exercise: Push-ups, 12 reps
        Assert.Equal(12, svc.PendingValue);
    }

    [Fact]
    public async Task Adjusting_pending_logs_actual_reps_not_target()
    {
        var (svc, _) = await MakeAsync(autoRest: false);
        svc.Start(CustomPlan(("pushup", 2, 12, null, 30)));   // 2 sets, target 12
        svc.DecPending(); svc.DecPending();                    // 12 -> 10
        svc.CompleteSet();                                     // set 1 logs 10
        Assert.Equal(12, svc.PendingValue);                    // reset to target for set 2
        svc.CompleteSet();                                     // set 2 logs 12 (ends)
        Assert.Equal(22, svc.LoggedSumAt(0));                  // 10 + 12 actual reps
        Assert.Equal(12, svc.BestSetAt(0));                    // best actual set
        Assert.Equal(22, svc.TotalRepsDone);                   // not 2*12=24 (the target)
    }

    [Fact]
    public async Task Pending_resets_to_target_on_advance()
    {
        var (svc, _) = await MakeAsync(autoRest: false);
        svc.Start(CustomPlan(("pushup", 1, 12, null, 30), ("squat", 1, 20, null, 30)));
        svc.CompleteSet();                                     // last set of pushup -> advance to squat
        Assert.Equal(1, svc.CurIdx);
        Assert.Equal(20, svc.PendingValue);                    // squat's target
    }

    [Fact]
    public async Task Hold_logs_seconds_and_best()
    {
        var (svc, _) = await MakeAsync(autoRest: false);
        svc.Start(CustomPlan(("plank", 2, null, 30, 30)));     // hold 30s, 2 sets
        Assert.Equal(30, svc.PendingValue);                    // hold target seconds
        svc.IncPending();                                      // holds step by 5 -> 35
        svc.CompleteSet();                                     // logs 35
        svc.CompleteSet();                                     // logs 30 (ends)
        Assert.Equal(35, svc.BestSetAt(0));                    // best actual hold
        Assert.Equal(0, svc.TotalRepsDone);                    // holds aren't reps
    }

    [Fact]
    public async Task CompleteSet_singleExercisePlan_ends_after_its_sets()
    {
        var (svc, _) = await MakeAsync(autoRest: false);
        svc.Start(CustomPlan(("plank", 2, null, 30, 30)));   // hold-type, 2 sets
        int finished = 0;
        svc.Finished += () => finished++;
        svc.CompleteSet();
        Assert.True(svc.Running);
        Assert.Equal(0, svc.CurIdx);
        svc.CompleteSet();                          // last set of only exercise → end
        Assert.False(svc.Running);
        Assert.Equal(1, finished);
    }
}
