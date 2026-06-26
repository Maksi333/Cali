using SQLite;

namespace Cali.Core.Models;

[Table("PersonalRecords")]
public sealed class PersonalRecord
{
    [PrimaryKey] public string ExerciseId { get; set; } = "";
    public int? MaxReps { get; set; }
    public int? LongestHoldSec { get; set; }
    public DateTime AchievedUtc { get; set; }
}
