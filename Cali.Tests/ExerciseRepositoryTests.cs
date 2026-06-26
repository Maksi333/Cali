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
        Assert.Equal(19, repo.All.Count);
        Assert.Equal("Push-ups", repo.Get("pushup")!.Name);
        Assert.Equal("Muscle-ups", repo.Get("muscleup")!.Name);
        Assert.Null(repo.Get("nope"));
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
