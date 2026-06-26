using Cali.Core.Models;
using Microsoft.Maui.Graphics;

namespace Cali.ViewModels;

/// <summary>Display wrapper for an <see cref="Exercise"/> shown as a library row.</summary>
public sealed class ExerciseRowViewModel
{
    public Exercise Exercise { get; }
    public ExerciseRowViewModel(Exercise exercise) => Exercise = exercise;

    public string Name => Exercise.Name;
    public string Sub => $"{Exercise.Primary} · {Exercise.Equipment}";
    public string DifficultyText => Exercise.Difficulty.ToString();
    public Color DifficultyColor => Exercise.Difficulty switch
    {
        Difficulty.Beginner => Tokens.Success,
        Difficulty.Advanced => Tokens.Danger,
        _ => Tokens.Accent
    };
}
