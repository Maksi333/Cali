using Cali.Core.Models;
using Cali.Core.Services;
using Cali.Tests.Fakes;
using Xunit;

public class ExerciseRepositoryTests
{
    private static async Task<ExerciseRepository> MakeAsync()
    {
        var repo = new ExerciseRepository(new FileSeedDataProvider());
        await repo.InitAsync();
        return repo;
    }

    [Fact]
    public async Task Loads_all_and_resolves_by_id()
    {
        var repo = await MakeAsync();
        Assert.Equal(90, repo.All.Count);  // 19 originals + 71 content-expansion exercises
        Assert.Equal("Push-ups", repo.Get("pushup")!.Name);
        Assert.Equal("Muscle-ups", repo.Get("muscleup")!.Name);
        Assert.Equal("One-Arm Push-ups", repo.Get("onearmpushup")!.Name);
        Assert.Null(repo.Get("nope"));
    }

    [Fact]
    public async Task Every_progression_link_resolves_and_ids_are_unique()
    {
        var repo = await MakeAsync();
        var ids = repo.All.Select(e => e.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());            // no duplicate ids
        foreach (var e in repo.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(e.Name), $"{e.Id} has no name");
            Assert.True(e.Cues.Count >= 3, $"{e.Id} should have at least 3 form cues");
            Assert.True(e.Mistakes.Count >= 2, $"{e.Id} should have at least 2 common mistakes");
            // Links are either blank, a self-reference (the seed's "no link this way" convention,
            // filtered out by the detail VM), or a resolvable id — never a dangling reference.
            if (e.EasierId != "") Assert.NotNull(repo.Get(e.EasierId));
            if (e.HarderId != "") Assert.NotNull(repo.Get(e.HarderId));
        }
    }

    [Fact]
    public async Task Families_assigned_for_volume_exercises()
    {
        var repo = await MakeAsync();
        Assert.Equal("pushup", repo.Get("hspu")!.Family);          // handstand push-ups count as push volume
        Assert.Equal("dip", repo.Get("benchdips")!.Family);
        Assert.Equal("pullup", repo.Get("negativepullup")!.Family);
        Assert.Equal("squat", repo.Get("bulgariansplit")!.Family);
        Assert.Equal("core", repo.Get("hollowrock")!.Family);
        Assert.Equal("", repo.Get("deadhang")!.Family);            // holds carry no rep-volume family
    }

    [Fact]
    public async Task Query_combines_muscle_difficulty_search()
    {
        var repo = await MakeAsync();
        Assert.All(repo.Query("Core", null, null), e => Assert.Equal("Core", e.Primary));
        Assert.All(repo.Query(null, Difficulty.Advanced, null), e => Assert.Equal(Difficulty.Advanced, e.Difficulty));
        Assert.Single(repo.Query(null, null, "diamond"));
        Assert.Empty(repo.Query("Legs", Difficulty.Advanced, "push"));
    }
}
