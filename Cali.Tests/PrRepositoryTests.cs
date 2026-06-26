using Cali.Core.Services;
using Xunit;

public class PrRepositoryTests : IAsyncLifetime
{
    private string _path = "";
    private Database _db = null!;
    private PrRepository _repo = null!;

    public async Task InitializeAsync()
    {
        _path = Path.Combine(Path.GetTempPath(), $"cali-{Guid.NewGuid():N}.db3");
        _db = new Database(_path); await _db.InitAsync();
        _repo = new PrRepository(_db);
    }

    public Task DisposeAsync() { try { File.Delete(_path); } catch { } return Task.CompletedTask; }

    [Fact]
    public async Task Reps_pr_only_beats_when_higher()
    {
        var utc = new DateTime(2026, 6, 23, 0, 0, 0, DateTimeKind.Utc);
        Assert.True(await _repo.TryBeatRepsAsync("pushup", 20, utc));   // first = PR
        Assert.False(await _repo.TryBeatRepsAsync("pushup", 18, utc));  // lower = no
        Assert.True(await _repo.TryBeatRepsAsync("pushup", 25, utc));   // higher = PR
        Assert.Equal(25, (await _repo.GetAsync("pushup"))!.MaxReps);
    }

    [Fact]
    public async Task Hold_pr_tracks_longest()
    {
        var utc = new DateTime(2026, 6, 23, 0, 0, 0, DateTimeKind.Utc);
        Assert.True(await _repo.TryBeatHoldAsync("plank", 45, utc));
        Assert.False(await _repo.TryBeatHoldAsync("plank", 30, utc));
        Assert.Equal(45, (await _repo.GetAsync("plank"))!.LongestHoldSec);
    }
}
