using SQLite;

namespace Cali.Core.Models;

[Table("WorkoutRecords")]
public sealed class WorkoutRecord
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public DateTime DateUtc { get; set; }
    public string PlanName { get; set; } = "";
    public int DurationSec { get; set; }
    public int ExerciseCount { get; set; }
    public int TotalSets { get; set; }
    public int TotalReps { get; set; }
    public Effort Effort { get; set; }
    public int PrCount { get; set; }
    public string ItemsJson { get; set; } = "";

    public List<WorkoutItemLog> Items()
    {
        if (string.IsNullOrEmpty(ItemsJson)) return new();
        try { return System.Text.Json.JsonSerializer.Deserialize<List<WorkoutItemLog>>(ItemsJson) ?? new(); }
        catch { return new(); }
    }

    public void SetItems(IEnumerable<WorkoutItemLog> items) =>
        ItemsJson = System.Text.Json.JsonSerializer.Serialize(items);
}
