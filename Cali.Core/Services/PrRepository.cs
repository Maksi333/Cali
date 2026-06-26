using Cali.Core.Models;

namespace Cali.Core.Services;

public sealed class PrRepository
{
    private readonly IDatabase _db;
    public PrRepository(IDatabase db) => _db = db;

    public async Task<PersonalRecord?> GetAsync(string exerciseId) =>
        await _db.Conn.FindAsync<PersonalRecord>(exerciseId);

    public Task<List<PersonalRecord>> GetAllAsync() => _db.Conn.Table<PersonalRecord>().ToListAsync();

    public async Task<bool> TryBeatRepsAsync(string exerciseId, int reps, DateTime utc)
    {
        var pr = await _db.Conn.FindAsync<PersonalRecord>(exerciseId);
        if (pr is not null && pr.MaxReps >= reps) return false;
        pr ??= new PersonalRecord { ExerciseId = exerciseId };
        pr.MaxReps = reps;
        pr.AchievedUtc = utc;
        await _db.Conn.InsertOrReplaceAsync(pr);
        return true;
    }

    public async Task<bool> TryBeatHoldAsync(string exerciseId, int holdSec, DateTime utc)
    {
        var pr = await _db.Conn.FindAsync<PersonalRecord>(exerciseId);
        if (pr is not null && pr.LongestHoldSec >= holdSec) return false;
        pr ??= new PersonalRecord { ExerciseId = exerciseId };
        pr.LongestHoldSec = holdSec;
        pr.AchievedUtc = utc;
        await _db.Conn.InsertOrReplaceAsync(pr);
        return true;
    }
}
