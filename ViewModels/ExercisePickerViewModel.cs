using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Cali.Core.Models;
using Cali.Core.Services;

namespace Cali.ViewModels;

public partial class ExercisePickerViewModel : ObservableObject
{
    private readonly ExerciseRepository _exercises;
    public ExercisePickerViewModel(ExerciseRepository exercises) => _exercises = exercises;

    public ObservableCollection<Exercise> Exercises { get; } = new();

    public void Load()
    {
        Exercises.Clear();
        foreach (var e in _exercises.All) Exercises.Add(e);
    }
}
