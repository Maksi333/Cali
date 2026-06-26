using Cali.Core.Models;
using Microsoft.Maui.Graphics;

namespace Cali.ViewModels;

/// <summary>Display wrapper for a <see cref="Plan"/> shown as a card on the Plans tab.</summary>
public sealed class PlanCardViewModel
{
    public Plan Plan { get; }
    public PlanCardViewModel(Plan plan) => Plan = plan;

    public string Name => Plan.Name;
    public string BadgeText => Plan.Preset ? "PRESET" : "CUSTOM";
    public Color BadgeColor => Plan.Preset ? Tokens.Accent : Tokens.TextMuted;
    public Color AccentBar => Plan.Preset ? Tokens.Accent : Tokens.CustomBar;

    public string DifficultyText => Plan.Difficulty.ToString();
    public Color DifficultyColor => Plan.Difficulty switch
    {
        Difficulty.Beginner => Tokens.Success,
        Difficulty.Advanced => Tokens.Danger,
        _ => Tokens.Accent
    };

    public string DurationText => $"{Plan.EstMinutes} min";
    public string MusclesText => Plan.Muscles;
}
