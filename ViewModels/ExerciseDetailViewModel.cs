using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cali.Core.Models;
using Cali.Core.Services;
using Microsoft.Maui.Graphics;

namespace Cali.ViewModels;

public partial class ExerciseDetailViewModel : ObservableObject
{
    private readonly ExerciseRepository _exercises;
    public ExerciseDetailViewModel(ExerciseRepository exercises) => _exercises = exercises;

    [ObservableProperty] private string name = "";
    [ObservableProperty] private string description = "";
    [ObservableProperty] private string primaryMuscle = "";
    [ObservableProperty] private string secondaryEquip = "";
    [ObservableProperty] private string difficultyText = "";
    [ObservableProperty] private Color difficultyColor = Tokens.Success;

    [ObservableProperty] private bool hasEasier;
    [ObservableProperty] private bool hasHarder;
    [ObservableProperty] private string easierName = "";
    [ObservableProperty] private string harderName = "";

    private string _easierId = "";
    private string _harderId = "";

    public ObservableCollection<string> Cues { get; } = new();
    public ObservableCollection<string> Mistakes { get; } = new();

    public void Load(string id)
    {
        var ex = _exercises.Get(id);
        if (ex is null) return;

        Name = ex.Name;
        Description = ex.Description;
        PrimaryMuscle = ex.Primary;
        SecondaryEquip = string.IsNullOrWhiteSpace(ex.Secondary)
            ? ex.Equipment
            : $"{ex.Secondary} · {ex.Equipment}";
        DifficultyText = ex.Difficulty.ToString();
        DifficultyColor = ex.Difficulty switch
        {
            Difficulty.Beginner => Tokens.Success,
            Difficulty.Advanced => Tokens.Danger,
            _ => Tokens.Accent
        };

        Cues.Clear();
        for (int i = 0; i < ex.Cues.Count; i++) Cues.Add($"{i + 1}.  {ex.Cues[i]}");
        Mistakes.Clear();
        foreach (var m in ex.Mistakes) Mistakes.Add($"✕  {m}");

        _easierId = ex.EasierId;
        _harderId = ex.HarderId;
        var easier = _exercises.Get(_easierId);
        var harder = _exercises.Get(_harderId);
        HasEasier = easier is not null && easier.Id != ex.Id;
        HasHarder = harder is not null && harder.Id != ex.Id;
        EasierName = easier?.Name ?? "";
        HarderName = harder?.Name ?? "";
    }

    // The signature feature: re-target the detail to the linked progression in place.
    [RelayCommand] private void GoEasier() { if (HasEasier) Load(_easierId); }
    [RelayCommand] private void GoHarder() { if (HasHarder) Load(_harderId); }
}
