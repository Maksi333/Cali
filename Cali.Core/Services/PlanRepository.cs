using Cali.Core.Models;

namespace Cali.Core.Services;

public sealed class PlanRepository
{
    private readonly IDatabase _db;
    public PlanRepository(IDatabase db) => _db = db;

    public async Task<List<Plan>> GetAllAsync()
    {
        var plans = await _db.Conn.Table<Plan>().ToListAsync();
        foreach (var p in plans) p.Exercises = await LoadExercisesAsync(p.Id);
        return plans;
    }

    public async Task<List<Plan>> GetPresetsAsync() => (await GetAllAsync()).Where(p => p.Preset).ToList();
    public async Task<List<Plan>> GetUserPlansAsync() => (await GetAllAsync()).Where(p => !p.Preset).ToList();

    public async Task<Plan?> GetByIdAsync(string id)
    {
        var p = await _db.Conn.FindAsync<Plan>(id);
        if (p is null) return null;
        p.Exercises = await LoadExercisesAsync(id);
        return p;
    }

    private async Task<List<PlanExercise>> LoadExercisesAsync(string planId) =>
        (await _db.Conn.Table<PlanExercise>().Where(x => x.PlanId == planId).ToListAsync())
        .OrderBy(x => x.Order).ToList();

    public async Task SaveAsync(Plan plan)
    {
        await _db.Conn.InsertOrReplaceAsync(plan);
        await _db.Conn.ExecuteAsync("DELETE FROM PlanExercises WHERE PlanId = ?", plan.Id);
        for (int i = 0; i < plan.Exercises.Count; i++)
        {
            var pe = plan.Exercises[i];
            pe.Id = 0;
            pe.PlanId = plan.Id;
            pe.Order = i;
            await _db.Conn.InsertAsync(pe);
        }
    }

    public async Task DeleteAsync(string id)
    {
        await _db.Conn.ExecuteAsync("DELETE FROM PlanExercises WHERE PlanId = ?", id);
        await _db.Conn.DeleteAsync<Plan>(id);
    }

    public async Task<Plan> DuplicateAsync(string id)
    {
        var src = await GetByIdAsync(id) ?? throw new InvalidOperationException($"Plan {id} not found");
        var copy = new Plan
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = src.Name + " (Copy)",
            Preset = false,
            Difficulty = src.Difficulty,
            EstMinutes = src.EstMinutes,
            Muscles = src.Muscles,
            Exercises = src.Exercises.Select(e => new PlanExercise
            {
                ExerciseId = e.ExerciseId,
                Order = e.Order,
                Sets = e.Sets,
                Reps = e.Reps,
                HoldSec = e.HoldSec,
                RestSec = e.RestSec
            }).ToList()
        };
        await SaveAsync(copy);
        return copy;
    }

    public async Task SeedPresetsAsync(List<SeedPlan> seedPlans)
    {
        foreach (var sp in seedPlans)
        {
            if (await _db.Conn.FindAsync<Plan>(sp.Id) is not null) continue; // idempotent
            var plan = new Plan
            {
                Id = sp.Id,
                Name = sp.Name,
                Preset = sp.Preset,
                Difficulty = Enum.Parse<Difficulty>(sp.Difficulty, true),
                EstMinutes = sp.EstMinutes,
                Muscles = sp.Muscles,
                Exercises = sp.Exercises.Select((e, i) => new PlanExercise
                {
                    ExerciseId = e.ExerciseId,
                    Order = i,
                    Sets = e.Sets,
                    Reps = e.Reps,
                    HoldSec = e.HoldSec,
                    RestSec = e.RestSec
                }).ToList()
            };
            await SaveAsync(plan);
        }
    }
}
