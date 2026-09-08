using Cali.Core.Models;
using Cali.Tests.Fakes;
using Xunit;

public class ModelsTests
{
    [Fact]
    public async Task SeedData_parses_expected_counts_and_links()
    {
        var json = await new FileSeedDataProvider().ReadSeedJsonAsync();
        var seed = SeedData.Parse(json);

        Assert.Equal(90, seed.Exercises.Count);   // 19 originals + 71 content-expansion exercises
        Assert.Equal(6, seed.PredefinedPlans.Count);
        Assert.Empty(seed.SampleHistory);   // BUG-004: no demo data ships in the seed
        Assert.Null(seed.SampleProfile);
        Assert.Equal(5, seed.Achievements.Count);

        var pushup = seed.Exercises.Single(e => e.Id == "pushup").ToExercise();
        Assert.Equal(ExerciseType.Reps, pushup.Type);
        Assert.Equal(Difficulty.Beginner, pushup.Difficulty);
        Assert.Equal("incline", pushup.EasierId);
        Assert.Equal("diamond", pushup.HarderId);

        // Muscle-up exercise gives the "first-muscleup" achievement a data path.
        var muscleup = seed.Exercises.Single(e => e.Id == "muscleup");
        Assert.Equal("Muscle-ups", muscleup.Name);
        Assert.Equal("muscleup", seed.Exercises.Single(e => e.Id == "pullup").HarderId);

        var plank = seed.Exercises.Single(e => e.Id == "plank").ToExercise();
        Assert.Equal(ExerciseType.Hold, plank.Type);

        // every non-blank easier/harder id resolves to a real exercise ("" means no link)
        var ids = seed.Exercises.Select(e => e.Id).ToHashSet();
        foreach (var e in seed.Exercises)
        {
            if (e.EasierId != "") Assert.Contains(e.EasierId, ids);
            if (e.HarderId != "") Assert.Contains(e.HarderId, ids);
        }
    }
}
