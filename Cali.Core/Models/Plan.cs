using SQLite;

namespace Cali.Core.Models;

[Table("Plans")]
public sealed class Plan
{
    [PrimaryKey] public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool Preset { get; set; }
    public Difficulty Difficulty { get; set; }
    public int EstMinutes { get; set; }
    public string Muscles { get; set; } = "";
    [Ignore] public List<PlanExercise> Exercises { get; set; } = new();
}

[Table("PlanExercises")]
public sealed class PlanExercise
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public string PlanId { get; set; } = "";
    public string ExerciseId { get; set; } = "";
    public int Order { get; set; }
    public int Sets { get; set; }
    public int? Reps { get; set; }
    public int? HoldSec { get; set; }
    public int RestSec { get; set; }
}
