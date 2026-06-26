using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cali.Core.Models;
using Cali.Core.Services;
using Cali.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Cali.ViewModels;

public partial class ExercisesViewModel : ObservableObject
{
    private readonly ExerciseRepository _exercises;
    private readonly IServiceProvider _services;

    public ExercisesViewModel(ExerciseRepository exercises, IServiceProvider services)
    {
        _exercises = exercises;
        _services = services;
        foreach (var m in new[] { "All", "Chest", "Back", "Shoulders", "Arms", "Core", "Legs", "Full Body" })
            MuscleChips.Add(new ChipViewModel(m, m == "All"));
        foreach (var d in new[] { "All", "Beginner", "Intermediate", "Advanced" })
            DifficultyChips.Add(new ChipViewModel(d, d == "All"));
    }

    public ObservableCollection<ChipViewModel> MuscleChips { get; } = new();
    public ObservableCollection<ChipViewModel> DifficultyChips { get; } = new();
    public ObservableCollection<ExerciseRowViewModel> Results { get; } = new();

    [ObservableProperty] private string search = "";
    [ObservableProperty] private string countText = "";

    private string _muscle = "All";
    private string _difficulty = "All";

    partial void OnSearchChanged(string value) => Apply();

    [RelayCommand]
    private void SelectMuscle(ChipViewModel chip)
    {
        foreach (var c in MuscleChips) c.Selected = ReferenceEquals(c, chip);
        _muscle = chip.Label;
        Apply();
    }

    [RelayCommand]
    private void SelectDifficulty(ChipViewModel chip)
    {
        foreach (var c in DifficultyChips) c.Selected = ReferenceEquals(c, chip);
        _difficulty = chip.Label;
        Apply();
    }

    [RelayCommand]
    private async Task Open(ExerciseRowViewModel? row)
    {
        if (row is null) return;
        var page = _services.GetRequiredService<ExerciseDetailPage>();
        page.Load(row.Exercise.Id);
        await Shell.Current.Navigation.PushModalAsync(page);
    }

    public void Apply()
    {
        Difficulty? diff = _difficulty == "All" ? null : Enum.Parse<Difficulty>(_difficulty);
        var list = _exercises.Query(_muscle, diff, Search);
        Results.Clear();
        foreach (var e in list) Results.Add(new ExerciseRowViewModel(e));
        CountText = $"{list.Count} exercise{(list.Count == 1 ? "" : "s")}";
    }
}
