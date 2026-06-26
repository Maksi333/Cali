using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cali.Core.Models;
using Cali.Core.Services;
using Cali.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Cali.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly PlanRepository _plans;
    private readonly HistoryRepository _history;
    private readonly PrRepository _prs;
    private readonly IServiceProvider _services;
    private Plan? _todayPlan;

    public HomeViewModel(SettingsService settings, PlanRepository plans, HistoryRepository history, PrRepository prs, IServiceProvider services)
    {
        _settings = settings;
        _plans = plans;
        _history = history;
        _prs = prs;
        _services = services;
    }

    [ObservableProperty] private string greeting = "";
    [ObservableProperty] private string userName = "";
    [ObservableProperty] private string avatar = "";
    [ObservableProperty] private int currentStreak;
    [ObservableProperty] private int bestStreak;
    [ObservableProperty] private bool hasPlan;
    [ObservableProperty] private string planName = "";
    [ObservableProperty] private string planMeta = "";
    [ObservableProperty] private string planMuscles = "";
    [ObservableProperty] private int weekSessions;
    [ObservableProperty] private string weekTime = "0m";
    [ObservableProperty] private int prCount;

    public async Task LoadAsync()
    {
        var p = _settings.Profile;
        UserName = p.Name;
        Avatar = Initials(p.Name);
        Greeting = GreetingFor(DateTime.Now.Hour);
        var (curStreak, bestStreak) = await _history.ComputeStreakAsync();
        CurrentStreak = curStreak;
        BestStreak = bestStreak;

        var presets = await _plans.GetPresetsAsync();
        _todayPlan = presets.FirstOrDefault(x => x.Id == "push-day") ?? presets.FirstOrDefault();
        if (_todayPlan is not null)
        {
            HasPlan = true;
            PlanName = _todayPlan.Name;
            PlanMeta = $"{_todayPlan.EstMinutes} min · {_todayPlan.Exercises.Count} exercises";
            PlanMuscles = _todayPlan.Muscles;
        }

        var now = DateTime.UtcNow;
        WeekSessions = await _history.CountThisWeekAsync(now);
        var recent = await _history.GetRecentAsync(100);
        var weekMin = recent.Where(r => r.DateUtc >= now.AddDays(-7)).Sum(r => r.DurationSec) / 60;
        WeekTime = $"{weekMin}m";
        PrCount = (await _prs.GetAllAsync()).Count(pr => pr.AchievedUtc >= now.AddDays(-7));
    }

    [RelayCommand]
    private async Task StartToday()
    {
        if (_todayPlan is null) return;
        var page = _services.GetRequiredService<ActiveWorkoutPage>();
        ((ActiveWorkoutViewModel)page.BindingContext).Start(_todayPlan);
        await Shell.Current.Navigation.PushModalAsync(page);
    }

    [RelayCommand]
    private async Task OpenProfile() => await Shell.Current.GoToAsync("//you");

    private static string GreetingFor(int hour) =>
        hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";

    private static string Initials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "??";
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2) return (parts[0][..1] + parts[1][..1]).ToUpperInvariant();
        var w = parts[0];
        return (w.Length >= 2 ? $"{w[0]}{w[^1]}" : w).ToUpperInvariant();
    }
}
