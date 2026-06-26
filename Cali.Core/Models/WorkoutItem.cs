namespace Cali.Core.Models;

/// <summary>A plan exercise resolved with its full <see cref="Exercise"/> for the live workout.</summary>
public sealed class WorkoutItem
{
    public required Exercise Exercise { get; init; }
    public int Sets { get; init; }
    public int? Reps { get; init; }
    public int? HoldSec { get; init; }
    public int RestSec { get; init; }
}
