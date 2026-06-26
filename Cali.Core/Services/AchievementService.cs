using Cali.Core.Models;

namespace Cali.Core.Services;

/// <summary>
/// Evaluates the 53-achievement catalog against live data, persists unlocks (once), and queues
/// newly-earned achievements for celebration. Idempotent + retroactive (safe to run repeatedly).
/// </summary>
public sealed class AchievementService
{
    private readonly IDatabase _db;
    private readonly HistoryRepository _history;
    private readonly PrRepository _prs;
    private readonly PlanRepository _plans;
    private readonly ExerciseRepository _exercises;

    public AchievementService(IDatabase db, HistoryRepository history, PrRepository prs, PlanRepository plans, ExerciseRepository exercises)
    {
        _db = db;
        _history = history;
        _prs = prs;
        _plans = plans;
        _exercises = exercises;
    }

    /// <summary>Newly-earned achievement ids awaiting a celebration toast.</summary>
    public List<string> PendingCelebrations { get; } = new();

    private async Task<StatsContext> BuildStatsAsync()
    {
        var history = await _history.GetRecentAsync(int.MaxValue);
        var prs = await _prs.GetAllAsync();
        var userPlans = await _plans.GetUserPlansAsync();
        var presets = await _plans.GetPresetsAsync();
        var presetNames = presets.Select(p => p.Name).ToHashSet();
        var exFamily = _exercises.All.ToDictionary(e => e.Id, e => e.Family);
        return StatsContextBuilder.Build(history, prs, userPlans.Count, presetNames, exFamily,
            TimeZoneInfo.Local, DateOnly.FromDateTime(DateTime.Now));
    }

    public async Task<List<string>> SyncAsync(DateTime nowUtc, bool celebrate)
    {
        var stats = await BuildStatsAsync();
        var met = AchievementCatalog.All.Where(a => CriteriaEvaluator.Passes(a.Criteria, stats)).Select(a => a.Id).ToHashSet();
        var already = (await _db.Conn.Table<AchievementUnlock>().ToListAsync()).Select(a => a.Id).ToHashSet();
        var newly = AchievementCatalog.All.Where(a => met.Contains(a.Id) && !already.Contains(a.Id)).Select(a => a.Id).ToList();

        foreach (var id in newly)
            await _db.Conn.InsertAsync(new AchievementUnlock { Id = id, UnlockedUtc = nowUtc });

        if (celebrate) PendingCelebrations.AddRange(newly);
        return newly;
    }

    public async Task<List<AchievementStatus>> GetStatusAsync()
    {
        var stats = await BuildStatsAsync();
        return AchievementCatalog.All.Select(a =>
        {
            var (cur, target) = CriteriaEvaluator.Progress(a.Criteria, stats);
            return new AchievementStatus
            {
                Id = a.Id,
                Emoji = a.Emoji,
                Name = a.Name,
                Desc = a.Description,
                Category = a.Category,
                Tier = a.Tier,
                Points = a.Points,
                Hidden = a.Hidden,
                Unlocked = CriteriaEvaluator.Passes(a.Criteria, stats),
                Cur = Math.Min(cur, target),
                Target = target
            };
        }).ToList();
    }
}
