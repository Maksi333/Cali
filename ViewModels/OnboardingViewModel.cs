using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cali.Core.Services;
using Microsoft.Maui.Graphics;

namespace Cali.ViewModels;

public partial class OnboardingViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    public OnboardingViewModel(SettingsService settings)
    {
        _settings = settings;
        Apply(0);
    }

    public event Action? Completed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Dot0))]
    [NotifyPropertyChangedFor(nameof(Dot1))]
    [NotifyPropertyChangedFor(nameof(Dot2))]
    [NotifyPropertyChangedFor(nameof(Dot3))]
    [NotifyPropertyChangedFor(nameof(Dot0W))]
    [NotifyPropertyChangedFor(nameof(Dot1W))]
    [NotifyPropertyChangedFor(nameof(Dot2W))]
    [NotifyPropertyChangedFor(nameof(Dot3W))]
    private int step;

    [ObservableProperty] private string kicker = "";
    [ObservableProperty] private string title = "";
    [ObservableProperty] private string body = "";
    [ObservableProperty] private string ctaText = "";
    [ObservableProperty] private bool showOptions;
    [ObservableProperty] private bool showCta = true;

    public ObservableCollection<OptionVM> Options { get; } = new();

    public Color Dot0 => Step == 0 ? Tokens.Accent : Tokens.Surface2;
    public Color Dot1 => Step == 1 ? Tokens.Accent : Tokens.Surface2;
    public Color Dot2 => Step == 2 ? Tokens.Accent : Tokens.Surface2;
    public Color Dot3 => Step == 3 ? Tokens.Accent : Tokens.Surface2;
    public double Dot0W => Step == 0 ? 22 : 8;
    public double Dot1W => Step == 1 ? 22 : 8;
    public double Dot2W => Step == 2 ? 22 : 8;
    public double Dot3W => Step == 3 ? 22 : 8;

    private void Apply(int s)
    {
        Step = s;
        Options.Clear();
        switch (s)
        {
            case 0:
                Kicker = "WELCOME"; Title = "Train anywhere.\nJust your body.";
                Body = "Build real strength with bodyweight calisthenics — no gym, no weights, no excuses.";
                CtaText = "Get started"; ShowOptions = false; ShowCta = true;
                break;
            case 1:
                Kicker = "STEP 1 OF 3"; Title = "What's your goal?"; Body = "We'll tailor your plan.";
                AddOptions(("Build strength", "Get stronger with progressions"),
                           ("Learn skills", "Handstands, muscle-ups & more"),
                           ("Lose fat", "Higher-volume conditioning"),
                           ("Mobility", "Move better, feel better"));
                CtaText = ""; ShowOptions = true; ShowCta = false;
                break;
            case 2:
                Kicker = "STEP 2 OF 3"; Title = "How experienced are you?"; Body = "Be honest — we'll scale it.";
                AddOptions(("Brand new", "Just starting out"),
                           ("Some", "I train occasionally"),
                           ("Experienced", "I train regularly"));
                CtaText = ""; ShowOptions = true; ShowCta = false;
                break;
            default:
                Kicker = "STEP 3 OF 3"; Title = "Push-up baseline"; Body = "How many can you do in one set?";
                AddOptions(("0–5", "We'll start with inclines"),
                           ("6–15", "A solid foundation"),
                           ("16+", "Strong — let's progress fast"));
                CtaText = "Build my plan"; ShowOptions = true; ShowCta = true;
                break;
        }
    }

    private void AddOptions(params (string label, string hint)[] opts)
    {
        foreach (var (label, hint) in opts) Options.Add(new OptionVM(label, hint));
    }

    [RelayCommand]
    private void Advance()
    {
        if (Step >= 3) Complete();
        else Apply(Step + 1);
    }

    // Remembers the user's onboarding picks so we can write them into the real profile.
    private string? _goal;
    private string? _level;

    [RelayCommand]
    private void SelectOption(OptionVM option)
    {
        switch (Step)
        {
            case 1: // Goal — must match ProfileViewModel.GoalOptions
                _goal = option.Label switch
                {
                    "Build strength" => "Strength",
                    "Learn skills" => "Skills",
                    "Lose fat" => "Fat loss",
                    _ => "Mobility"
                };
                break;
            case 2: // Experience — must match ProfileViewModel.LevelOptions
                _level = option.Label switch
                {
                    "Brand new" => "Beginner",
                    "Some" => "Intermediate",
                    _ => "Advanced"
                };
                break;
        }
        Advance();
    }

    [RelayCommand]
    private void Skip() => Complete();

    private void Complete()
    {
        // Persist the real selections so a fresh user's profile reflects their choices
        // (push-up baseline in step 3 is intentionally NOT mapped to MaxPullups — different movement).
        var p = _settings.Profile;
        if (_goal is not null) p.Goal = _goal;
        if (_level is not null) p.Level = _level;
        _settings.Profile = p;

        _settings.IsOnboardingComplete = true;
        Completed?.Invoke();
    }
}

public sealed class OptionVM
{
    public string Label { get; }
    public string Hint { get; }
    public OptionVM(string label, string hint) { Label = label; Hint = hint; }
}
