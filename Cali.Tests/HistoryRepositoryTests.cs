using Cali.Core.Models;
using Cali.Core.Services;
using Xunit;

public class HistoryRepositoryTests : IAsyncLifetime
{
    private string _path = "";
    private Database _db = null!;
    private HistoryRepository _repo = null!;

    public async Task InitializeAsync()
    {
        _path = Path.Combine(Path.GetTempPath(), $"cali-{Guid.NewGuid():N}.db3");
        _db = new Database(_path); await _db.InitAsync();
        _repo = new HistoryRepository(_db);
    }

    public Task DisposeAsync() { try { File.Delete(_path); } catch { } return Task.CompletedTask; }

    [Fact]
    public async Task Add_and_query_recent_and_week_and_reps()
    {
        var now = new DateTime(2026, 6, 23, 12, 0, 0, DateTimeKind.Utc);
        await _repo.AddAsync(new WorkoutRecord { DateUtc = now.AddDays(-1), PlanName = "Push Day", DurationSec = 1600, ExerciseCount = 6, TotalSets = 18, TotalReps = 120, Effort = Effort.JustRight });
        await _repo.AddAsync(new WorkoutRecord { DateUtc = now.AddDays(-10), PlanName = "Leg Day", DurationSec = 1200, ExerciseCount = 4, TotalSets = 12, TotalReps = 90, Effort = Effort.TooHard });

        var recent = await _repo.GetRecentAsync(10);
        Assert.Equal(2, recent.Count);
        Assert.Equal("Push Day", recent[0].PlanName); // newest first
        Assert.Equal(1, await _repo.CountThisWeekAsync(now));
        Assert.Equal(210, await _repo.TotalRepsAsync());
    }
}
