using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cali.Core.Models;
using Cali.Core.Services;
using Cali.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Cali.ViewModels;

public partial class PlanBuilderViewModel : ObservableObject
{
    private readonly PlanRepository _plans;
    private readonly ExerciseRepository _exercises;
    private readonly IServiceProvider _services;
    private string? _editId;

    public PlanBuilderViewModel(PlanRepository plans, ExerciseRepository exercises, IServiceProvider services)
    {
        _plans = plans;
        _exercises = exercises;
        _services = services;
        Rows.CollectionChanged += (_, _) => RefreshSummary();
    }

    public ObservableCollection<BuilderExerciseRow> Rows { get; } = new();

    [ObservableProperty] private string planName = "My Plan";
    [ObservableProperty] private string title = "New plan";
    [ObservableProperty] private string summary = "0 exercises";

    public event Action? CloseRequested;
    public event Action? Saved;

    public void NewPlan()
    {
        _editId = null;
        Title = "New plan";
        PlanName = "My Plan";
        ClearRows();
        RefreshSummary();
    }

    public void EditPlan(Plan p)
    {
        _editId = p.Id;
        Title = "Edit plan";
        PlanName = p.Name;
        ClearRows();
        foreach (var pe in p.Exercises)
        {
            var ex = _exercises.Get(pe.ExerciseId);
            if (ex is not null) AddRow(new BuilderExerciseRow(ex, pe.Sets, pe.Reps, pe.HoldSec, pe.RestSec));
        }
        RefreshSummary();
    }

    private void ClearRows()
    {
        foreach (var r in Rows) r.PropertyChanged -= OnRowChanged;
        Rows.Clear();
    }

    private void AddRow(BuilderExerciseRow row)
    {
        row.PropertyChanged += OnRowChanged;
        Rows.Add(row);
    }

    private void OnRowChanged(object? sender, PropertyChangedEventArgs e) => RefreshSummary();

    public void AddExerciseById(string exerciseId)
    {
        var ex = _exercises.Get(exerciseId);
        if (ex is null) return;
        var defaultRest = 60;
        AddRow(ex.Type == ExerciseType.Hold
            ? new BuilderExerciseRow(ex, 3, null, 30, defaultRest)
            : new BuilderExerciseRow(ex, 3, 10, null, defaultRest));
        RefreshSummary();
    }

    private void RefreshSummary()
    {
        var mins = (int)Math.Round(Rows.Sum(r => r.EstSeconds) / 60.0);
        var muscles = string.Join(" · ", Rows
            .Select(r => _exercises.Get(r.ExerciseId)?.Primary)
            .Where(m => !string.IsNullOrEmpty(m))
            .Distinct());
        Summary = Rows.Count == 0
            ? "0 exercises"
            : $"~{mins} min · {Rows.Count} exercise{(Rows.Count == 1 ? "" : "s")}{(string.IsNullOrEmpty(muscles) ? "" : " · " + muscles)}";
    }

    // ---- Steppers (clamped) ----
    [RelayCommand] private void IncSets(BuilderExerciseRow r) => r.Sets = Math.Min(r.Sets + 1, 8);
    [RelayCommand] private void DecSets(BuilderExerciseRow r) => r.Sets = Math.Max(r.Sets - 1, 1);
    [RelayCommand] private void IncTarget(BuilderExerciseRow r) { if (r.IsHold) r.HoldSec = Math.Min(r.HoldSec + 5, 300); else r.Reps = Math.Min(r.Reps + 1, 60); }
    [RelayCommand] private void DecTarget(BuilderExerciseRow r) { if (r.IsHold) r.HoldSec = Math.Max(r.HoldSec - 5, 5); else r.Reps = Math.Max(r.Reps - 1, 1); }
    [RelayCommand] private void IncRest(BuilderExerciseRow r) => r.RestSec = Math.Min(r.RestSec + 15, 180);
    [RelayCommand] private void DecRest(BuilderExerciseRow r) => r.RestSec = Math.Max(r.RestSec - 15, 0);

    [RelayCommand]
    private void Remove(BuilderExerciseRow r)
    {
        r.PropertyChanged -= OnRowChanged;
        Rows.Remove(r);
    }

    [RelayCommand]
    private void MoveUp(BuilderExerciseRow r)
    {
        var i = Rows.IndexOf(r);
        if (i > 0) Rows.Move(i, i - 1);
    }

    [RelayCommand]
    private void MoveDown(BuilderExerciseRow r)
    {
        var i = Rows.IndexOf(r);
        if (i >= 0 && i < Rows.Count - 1) Rows.Move(i, i + 1);
    }

    [RelayCommand]
    private async Task AddExercise()
    {
        var picker = _services.GetRequiredService<ExercisePickerPage>();
        picker.OnPicked = id => AddExerciseById(id);
        await Shell.Current.Navigation.PushModalAsync(picker);
    }

    [RelayCommand]
    private async Task Save()
    {
        if (Rows.Count == 0)
        {
            await Shell.Current.DisplayAlert("Add an exercise", "A plan needs at least one exercise.", "OK");
            return;
        }

        var difficulty = Rows
            .Select(r => _exercises.Get(r.ExerciseId)?.Difficulty ?? Difficulty.Beginner)
            .DefaultIfEmpty(Difficulty.Beginner)
            .Max();
        var muscles = string.Join(" · ", Rows
            .Select(r => _exercises.Get(r.ExerciseId)?.Primary)
            .Where(m => !string.IsNullOrEmpty(m))
            .Distinct());

        var plan = new Plan
        {
            Id = _editId ?? Guid.NewGuid().ToString("N"),
            Name = string.IsNullOrWhiteSpace(PlanName) ? "My Plan" : PlanName.Trim(),
            Preset = false,
            Difficulty = difficulty,
            EstMinutes = (int)Math.Round(Rows.Sum(r => r.EstSeconds) / 60.0),
            Muscles = muscles,
            Exercises = Rows.Select((r, i) => new PlanExercise
            {
                ExerciseId = r.ExerciseId,
                Order = i,
                Sets = r.Sets,
                Reps = r.IsHold ? null : r.Reps,
                HoldSec = r.IsHold ? r.HoldSec : null,
                RestSec = r.RestSec
            }).ToList()
        };

        await _plans.SaveAsync(plan);
        Saved?.Invoke();
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke();
}
