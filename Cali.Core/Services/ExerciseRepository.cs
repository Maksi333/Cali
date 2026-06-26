using Cali.Core.Abstractions;
using Cali.Core.Models;

namespace Cali.Core.Services;

public sealed class ExerciseRepository
{
    private readonly ISeedDataProvider _seed;
    private List<Exercise> _all = new();
    public ExerciseRepository(ISeedDataProvider seed) => _seed = seed;

    public async Task InitAsync()
    {
        var json = await _seed.ReadSeedJsonAsync();
        _all = SeedData.Parse(json).Exercises.Select(e => e.ToExercise()).ToList();
    }

    public IReadOnlyList<Exercise> All => _all;
    public Exercise? Get(string id) => _all.FirstOrDefault(e => e.Id == id);

    public IReadOnlyList<Exercise> Query(string? muscle, Difficulty? diff, string? search)
    {
        IEnumerable<Exercise> q = _all;
        if (!string.IsNullOrEmpty(muscle) && muscle != "All")
            q = q.Where(e => e.Primary == muscle);
        if (diff is { } d)
            q = q.Where(e => e.Difficulty == d);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        return q.ToList();
    }
}
