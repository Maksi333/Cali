using Cali.Core.Models;
using Cali.Core.Services;
using Cali.Tests.Fakes;
using Xunit;

public class PlanRepositoryTests : IAsyncLifetime
{
    private string _path = "";
    private Database _db = null!;
    private PlanRepository _repo = null!;

    public async Task InitializeAsync()
    {
        _path = Path.Combine(Path.GetTempPath(), $"cali-{Guid.NewGuid():N}.db3");
        _db = new Database(_path);
        await _db.InitAsync();
        _repo = new PlanRepository(_db);
        var seed = SeedData.Parse(await new FileSeedDataProvider().ReadSeedJsonAsync());
        await _repo.SeedPresetsAsync(seed.PredefinedPlans);
    }

    public Task DisposeAsync() { try { File.Delete(_path); } catch { } return Task.CompletedTask; }

    [Fact]
    public async Task Seeds_six_presets_with_exercises()
    {
        var presets = await _repo.GetPresetsAsync();
        Assert.Equal(6, presets.Count);
        var push = await _repo.GetByIdAsync("push-day");
        Assert.NotNull(push);
        Assert.Equal(6, push!.Exercises.Count);
        Assert.Equal("pushup", push.Exercises[0].ExerciseId);
    }

    [Fact]
    public async Task Seeding_is_idempotent()
    {
        var seed = SeedData.Parse(await new FileSeedDataProvider().ReadSeedJsonAsync());
        await _repo.SeedPresetsAsync(seed.PredefinedPlans); // second call
        Assert.Equal(6, (await _repo.GetPresetsAsync()).Count);
    }

    [Fact]
    public async Task Duplicate_creates_editable_user_plan()
    {
        var dup = await _repo.DuplicateAsync("push-day");
        Assert.False(dup.Preset);
        Assert.Contains("Copy", dup.Name);
        Assert.Equal(6, dup.Exercises.Count);
        var users = await _repo.GetUserPlansAsync();
        Assert.Single(users);
    }

    [Fact]
    public async Task Save_then_delete_user_plan()
    {
        var p = new Plan
        {
            Id = "u1", Name = "My Flow", Preset = false, Difficulty = Difficulty.Beginner,
            EstMinutes = 10, Muscles = "Full Body",
            Exercises = { new PlanExercise { ExerciseId = "squat", Order = 0, Sets = 3, Reps = 12, RestSec = 60 } }
        };
        await _repo.SaveAsync(p);
        Assert.NotNull(await _repo.GetByIdAsync("u1"));
        await _repo.DeleteAsync("u1");
        Assert.Null(await _repo.GetByIdAsync("u1"));
    }
}
