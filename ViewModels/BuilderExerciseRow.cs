using CommunityToolkit.Mvvm.ComponentModel;
using Cali.Core.Models;

namespace Cali.ViewModels;

/// <summary>An editable exercise row in the Plan Builder (sets / reps-or-hold / rest steppers).</summary>
public partial class BuilderExerciseRow : ObservableObject
{
    public string ExerciseId { get; }
    public string Name { get; }
    public bool IsHold { get; }

    [ObservableProperty] private int sets;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TargetValue))]
    private int reps;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TargetValue))]
    private int holdSec;

    [ObservableProperty] private int restSec;

    public BuilderExerciseRow(Exercise ex, int sets, int? reps, int? holdSec, int restSec)
    {
        ExerciseId = ex.Id;
        Name = ex.Name;
        IsHold = ex.Type == ExerciseType.Hold;
        this.sets = sets;
        this.reps = reps ?? 10;
        this.holdSec = holdSec ?? 30;
        this.restSec = restSec;
    }

    public string TargetLabel => IsHold ? "HOLD" : "REPS";
    public int TargetValue => IsHold ? HoldSec : Reps;

    /// <summary>Rough seconds this row contributes to the plan estimate.</summary>
    public int EstSeconds => Sets * ((IsHold ? HoldSec : Reps * 3) + RestSec);
}
