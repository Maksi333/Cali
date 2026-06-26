using Cali.Core.Models;

namespace Cali.Core.Services;

public sealed class HistoryRepository
{
    private readonly IDatabase _db;
    public HistoryRepository(IDatabase db) => _db = db;

    public Task AddAsync(WorkoutRecord r) => _db.Conn.InsertAsync(r);

    public async Task<List<WorkoutRecord>> GetRecentAsync(int n) =>
        (await _db.Conn.Table<WorkoutRecord>().ToListAsync())
        .OrderByDescending(r => r.DateUtc).Take(n).ToList();

    public async Task<int> CountThisWeekAsync(DateTime nowUtc)
    {
        var weekAgo = nowUtc.AddDays(-7);
        return (await _db.Conn.Table<WorkoutRecord>().ToListAsync())
            .Count(r => r.DateUtc >= weekAgo && r.DateUtc <= nowUtc);
    }

    public async Task<int> TotalRepsAsync() =>
        (await _db.Conn.Table<WorkoutRecord>().ToListAsync()).Sum(r => r.TotalReps);

    /// <summary>Real (current, best) consecutive-day streak from history, in local time.</summary>
    public async Task<(int current, int best)> ComputeStreakAsync()
    {
        var all = await _db.Conn.Table<WorkoutRecord>().ToListAsync();
        var days = all.Select(r => DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(r.DateUtc, DateTimeKind.Utc), TimeZoneInfo.Local)));
        return StreakCalculator.Compute(days, DateOnly.FromDateTime(DateTime.Now));
    }
}
