using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cali.Core.Services;
using Cali.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics;

namespace Cali.ViewModels;

public partial class PlansViewModel : ObservableObject
{
    private readonly PlanRepository _plans;
    private readonly IServiceProvider _services;

    public PlansViewModel(PlanRepository plans, IServiceProvider services)
    {
        _plans = plans;
        _services = services;
    }

    public ObservableCollection<PlanCardViewModel> Cards { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PredefinedBg))]
    [NotifyPropertyChangedFor(nameof(PredefinedText))]
    [NotifyPropertyChangedFor(nameof(MineBg))]
    [NotifyPropertyChangedFor(nameof(MineText))]
    [NotifyPropertyChangedFor(nameof(EmptyHeadline))]
    [NotifyPropertyChangedFor(nameof(EmptySub))]
    private bool showingPredefined = true;

    public Color PredefinedBg => ShowingPredefined ? Tokens.Accent : Tokens.Clear;
    public Color PredefinedText => ShowingPredefined ? Tokens.AccentOn : Tokens.TextMuted;
    public Color MineBg => ShowingPredefined ? Tokens.Clear : Tokens.Accent;
    public Color MineText => ShowingPredefined ? Tokens.TextMuted : Tokens.AccentOn;

    // ---- Empty state (BUG-009) ----
    [ObservableProperty] private bool isEmpty;
    public string EmptyHeadline => ShowingPredefined ? "No plans here yet" : "No plans of your own yet";
    public string EmptySub => ShowingPredefined
        ? "Build your first routine to get started."
        : "Create a custom plan, or duplicate a predefined one to tweak it.";

    public async Task LoadAsync()
    {
        var list = ShowingPredefined ? await _plans.GetPresetsAsync() : await _plans.GetUserPlansAsync();
        Cards.Clear();
        foreach (var p in list) Cards.Add(new PlanCardViewModel(p));
        IsEmpty = Cards.Count == 0;
    }

    [RelayCommand]
    private async Task ShowPredefined()
    {
        if (ShowingPredefined) return;
        ShowingPredefined = true;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ShowMine()
    {
        if (!ShowingPredefined) return;
        ShowingPredefined = false;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task StartPlan(PlanCardViewModel? card)
    {
        if (card?.Plan is null) return;
        var page = _services.GetRequiredService<ActiveWorkoutPage>();
        ((ActiveWorkoutViewModel)page.BindingContext).Start(card.Plan);
        await Shell.Current.Navigation.PushModalAsync(page);
    }

    [RelayCommand]
    private async Task NewPlan()
    {
        var page = _services.GetRequiredService<PlanBuilderPage>();
        page.Saved += OnPlanSaved;
        page.StartNew();
        await Shell.Current.Navigation.PushModalAsync(page);
    }

    [RelayCommand]
    private async Task Overflow(PlanCardViewModel? card)
    {
        if (card?.Plan is null) return;
        var plan = card.Plan;

        string action = plan.Preset
            ? await Shell.Current.DisplayActionSheet(plan.Name, "Cancel", null, "Duplicate to My plans")
            : await Shell.Current.DisplayActionSheet(plan.Name, "Cancel", "Delete", "Edit", "Duplicate");

        switch (action)
        {
            case "Edit":
                var page = _services.GetRequiredService<PlanBuilderPage>();
                page.Saved += OnPlanSaved;
                page.StartEdit(await _plans.GetByIdAsync(plan.Id) ?? plan);
                await Shell.Current.Navigation.PushModalAsync(page);
                break;

            case "Duplicate":
            case "Duplicate to My plans":
                await _plans.DuplicateAsync(plan.Id);
                ShowingPredefined = false;
                await LoadAsync();
                break;

            case "Delete":
                if (await Shell.Current.DisplayAlert("Delete plan", $"Delete “{plan.Name}”?", "Delete", "Cancel"))
                {
                    await _plans.DeleteAsync(plan.Id);
                    await LoadAsync();
                }
                break;
        }
    }

    private async void OnPlanSaved()
    {
        ShowingPredefined = false;
        await LoadAsync();
    }
}
