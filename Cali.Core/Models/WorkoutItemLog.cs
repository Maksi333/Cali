namespace Cali.Core.Models;

/// <summary>Per-exercise breakdown stored with a completed workout (serialized into WorkoutRecord.ItemsJson).</summary>
public sealed class WorkoutItemLog
{
    public string ExerciseId { get; set; } = "";
    public string Family { get; set; } = "";
    public string Muscle { get; set; } = "";
    public int Reps { get; set; }        // total reps performed (setsDone × repsPerSet)
    public int Sets { get; set; }        // sets completed
    public int BestSetReps { get; set; } // reps in a single set (per-set target)
    public int HoldSec { get; set; }     // hold seconds (isometrics)
}
