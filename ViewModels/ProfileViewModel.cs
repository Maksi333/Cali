using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cali.Core.Models;
using Cali.Core.Services;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace Cali.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly BackupService _backup;
    private readonly AchievementService _achievements;
    private AppSettings _s = new();
    private bool _loading;

    public ProfileViewModel(SettingsService settings, BackupService backup, AchievementService achievements)
    {
        _settings = settings;
        _backup = backup;
        _achievements = achievements;
    }

    [ObservableProperty] private string name = "";
    [ObservableProperty] private string avatar = "";
    [ObservableProperty] private string goalLevel = "";
    [ObservableProperty] private string bodyweight = "";
    [ObservableProperty] private string maxPullups = "";

    [ObservableProperty] private bool soundAlerts;
    [ObservableProperty] private bool haptics;
    [ObservableProperty] private bool autoStartRest;
    [ObservableProperty] private bool autoProgression;
    [ObservableProperty] private bool reminders;

    // ---- Edit mode (BUG-002) ----
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotEditing))]
    private bool isEditing;
    public bool IsNotEditing => !IsEditing;

    // String-backed edit buffers (bound to Entries; parsed/validated on save).
    [ObservableProperty] private string editName = "";
    [ObservableProperty] private string editBodyweight = "";   // kg
    [ObservableProperty] private string editMaxPullups = "";
    [ObservableProperty] private string editGoal = "Strength";
    [ObservableProperty] private string editLevel = "Beginner";

    public ObservableCollection<string> GoalOptions { get; } = new() { "Strength", "Skills", "Fat loss", "Mobility" };
    public ObservableCollection<string> LevelOptions { get; } = new() { "Beginner", "Intermediate", "Advanced" };

    // Sensible input bounds; bodyweight is standardised to kilograms.
    private const int MaxBodyweightKg = 500;
    private const int MaxPullupsCount = 1000;

    public void Load()
    {
        if (IsEditing) return; // don't clobber an in-progress edit on re-appear
        _loading = true;
        var p = _settings.Profile;
        _s = _settings.Settings;
        Name = p.Name;
        Avatar = Initials(p.Name);
        GoalLevel = $"Goal: {p.Goal} · {p.Level}";
        Bodyweight = $"{p.BodyweightKg} kg";
        MaxPullups = p.MaxPullups.ToString();
        SoundAlerts = _s.SoundAlerts;
        Haptics = _s.Haptics;
        AutoStartRest = _s.AutoStartRest;
        AutoProgression = _s.AutoProgression;
        Reminders = _s.Reminders;
        _loading = false;
    }

    [RelayCommand]
    private void BeginEdit()
    {
        var p = _settings.Profile;
        EditName = p.Name;
        EditGoal = GoalOptions.Contains(p.Goal) ? p.Goal : GoalOptions[0];
        EditLevel = LevelOptions.Contains(p.Level) ? p.Level : LevelOptions[0];
        EditBodyweight = p.BodyweightKg.ToString();
        EditMaxPullups = p.MaxPullups.ToString();
        IsEditing = true;
    }

    [RelayCommand]
    private void CancelEdit() => IsEditing = false;

    [RelayCommand]
    private void SaveEdit()
    {
        _settings.Profile = new Profile
        {
            Name = string.IsNullOrWhiteSpace(EditName) ? "Athlete" : EditName.Trim(),
            Goal = string.IsNullOrWhiteSpace(EditGoal) ? "Strength" : EditGoal,
            Level = string.IsNullOrWhiteSpace(EditLevel) ? "Beginner" : EditLevel,
            BodyweightKg = ProfileMath.ParseBounded(EditBodyweight, 0, MaxBodyweightKg),
            MaxPullups = ProfileMath.ParseBounded(EditMaxPullups, 0, MaxPullupsCount)
        };
        IsEditing = false; // allow Load() to refresh the read-only header (incl. initials)
        Load();
    }

    private void Persist()
    {
        if (_loading) return;
        _s.SoundAlerts = SoundAlerts;
        _s.Haptics = Haptics;
        _s.AutoStartRest = AutoStartRest;
        _s.AutoProgression = AutoProgression;
        _s.Reminders = Reminders;
        _settings.Save(_s);
    }

    partial void OnSoundAlertsChanged(bool value) => Persist();
    partial void OnHapticsChanged(bool value) => Persist();
    partial void OnAutoStartRestChanged(bool value) => Persist();
    partial void OnAutoProgressionChanged(bool value) => Persist();
    partial void OnRemindersChanged(bool value) => Persist();

    [RelayCommand]
    private async Task Export()
    {
        var json = await _backup.ExportJsonAsync();
        var file = Path.Combine(FileSystem.CacheDirectory, "cali-backup.json");
        await File.WriteAllTextAsync(file, json);
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Cali backup",
            File = new ShareFile(file)
        });
    }

    [RelayCommand]
    private async Task Import()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Pick a Cali backup" });
        if (result is null) return;
        var json = await File.ReadAllTextAsync(result.FullPath);
        await _backup.ImportJsonAsync(json);
        await _achievements.SyncAsync(DateTime.UtcNow, celebrate: false);
        Load();
        await Shell.Current.DisplayAlert("Import complete", "Your backup was imported.", "OK");
    }

    private static string Initials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "??";
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2) return (parts[0][..1] + parts[1][..1]).ToUpperInvariant();
        var w = parts[0];
        return (w.Length >= 2 ? $"{w[0]}{w[^1]}" : w).ToUpperInvariant();
    }
}
