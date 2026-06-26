using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cali.Core.Abstractions;
using Cali.Core.Models;
using Cali.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics;

namespace Cali.ViewModels;

public partial class ProgressViewModel : ObservableObject
{
    private readonly HistoryRepository _history;
    private readonly SettingsService _settings;
    private readonly AchievementService _achievements;
    private readonly IServiceProvider _services;

    public ProgressViewModel(HistoryRepository history, SettingsService settings, AchievementService achievements, IServiceProvider services)
    {
        _history = history;
        _settings = settings;
        _achievements = achievements;
        _services = services;
    }

    [RelayCommand]
    private async Task OpenAchievements()
    {
        var page = _services.GetRequiredService<Views.AchievementsPage>();
        await Shell.Current.Navigation.PushModalAsync(page);
    }

    [RelayCommand]
    private async Task StartFirst() => await Shell.Current.GoToAsync("//plans");

    // Empty state (BUG-009): no completed workouts yet.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasData))]
    private bool isEmpty;
    public bool HasData => !IsEmpty;

    [ObservableProperty] private int weekSessions;
    [ObservableProperty] private string totalVolume = "0";
    [ObservableProperty] private string timeTrained = "0";
    [ObservableProperty] private int longestStreak;
    [ObservableProperty] private string trendText = "this week";
    [ObservableProperty] private double[] weekValues = new double[7];
    [ObservableProperty] private string[] dayLabels = { "M", "T", "W", "T", "F", "S", "S" };
    [ObservableProperty] private int highlightDay = 6;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(SeeAllText))] private int achUnlocked;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(SeeAllText))] private int achTotal;
    public string SeeAllText => $"{AchUnlocked}/{AchTotal} · See all ›";

    public ObservableCollection<AchievementVM> Achievements { get; } = new();
    public ObservableCollection<RecentVM> Recent { get; } = new();

    public async Task LoadAsync()
    {
        var now = DateTime.UtcNow;
        var all = await _history.GetRecentAsync(int.MaxValue);
        IsEmpty = all.Count == 0;

        WeekSessions = all.Count(r => r.DateUtc >= now.AddDays(-7));
        var totalReps = all.Sum(r => r.TotalReps);
        TotalVolume = totalReps >= 1000 ? $"{totalReps / 1000.0:0.0}k" : totalReps.ToString();
        TimeTrained = $"{all.Sum(r => r.DurationSec) / 3600.0:0.0}";
        var streakDays = all.Select(r => DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(r.DateUtc, DateTimeKind.Utc), TimeZoneInfo.Local)));
        LongestStreak = StreakCalculator.Compute(streakDays, DateOnly.FromDateTime(DateTime.Now)).best;

        var vals = new double[7];
        var labels = new string[7];
        for (int i = 0; i < 7; i++)
        {
            var day = now.Date.AddDays(-(6 - i));
            vals[i] = all.Where(r => r.DateUtc.Date == day).Sum(r => r.TotalReps);
            labels[i] = day.DayOfWeek.ToString()[..1];
        }
        WeekValues = vals;
        DayLabels = labels;
        HighlightDay = 6;

        var thisWk = all.Where(r => r.DateUtc >= now.AddDays(-7)).Sum(r => r.TotalReps);
        var lastWk = all.Where(r => r.DateUtc >= now.AddDays(-14) && r.DateUtc < now.AddDays(-7)).Sum(r => r.TotalReps);
        if (lastWk > 0)
        {
            var pct = (int)Math.Round((thisWk - lastWk) * 100.0 / lastWk);
            TrendText = $"{(pct >= 0 ? "↑" : "↓")}{Math.Abs(pct)}% vs last wk"; // arrow matches sign (no "↑-43%")
        }
        else TrendText = "this week";

        var statuses = await _achievements.GetStatusAsync();
        AchTotal = statuses.Count;
        AchUnlocked = statuses.Count(s => s.Unlocked);
        Achievements.Clear();
        foreach (var s in statuses.OrderByDescending(s => s.Unlocked).Take(6))
        {
            bool mask = s.Hidden && !s.Unlocked;
            Achievements.Add(new AchievementVM(mask ? "❓" : s.Emoji, mask ? "???" : s.Name, s.Unlocked));
        }

        Recent.Clear();
        foreach (var r in all.OrderByDescending(x => x.DateUtc).Take(10))
            Recent.Add(new RecentVM(r));
    }
}

public sealed class AchievementVM
{
    public string Icon { get; }
    public string Name { get; }
    public bool Unlocked { get; }
    public double ItemOpacity => Unlocked ? 1.0 : 0.35;
    public AchievementVM(string icon, string name, bool unlocked)
    {
        Icon = icon; Name = name; Unlocked = unlocked;
    }
}

public sealed class RecentVM
{
    public string DateChip { get; }
    public string Name { get; }
    public string Meta { get; }
    public string EffortText { get; }
    public Color EffortColor { get; }

    public RecentVM(WorkoutRecord r)
    {
        DateChip = r.DateUtc.ToString("MMM d");
        Name = r.PlanName;
        Meta = $"{r.DurationSec / 60} min · {r.ExerciseCount} exercises";
        (EffortText, EffortColor) = r.Effort switch
        {
            Effort.TooEasy => ("Too easy", Tokens.Success),
            Effort.TooHard => ("Too hard", Tokens.Danger),
            _ => ("Just right", Tokens.Accent)
        };
    }
}
