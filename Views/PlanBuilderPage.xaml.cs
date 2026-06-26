using Cali.Core.Models;
using Cali.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cali.Views;

public partial class PlanBuilderPage : ContentPage
{
    private readonly PlanBuilderViewModel _vm;

    public event Action? Saved;

    public PlanBuilderPage()
    {
        InitializeComponent();
        _vm = IPlatformApplication.Current!.Services.GetRequiredService<PlanBuilderViewModel>();
        BindingContext = _vm;
    }

    public void StartNew() => _vm.NewPlan();
    public void StartEdit(Plan plan) => _vm.EditPlan(plan);

    private void OnSaved() => Saved?.Invoke();
    private async void OnClose() => await Navigation.PopModalAsync();

    // Re-subscribe on every appear so handlers survive the exercise-picker modal
    // covering this page (which fires OnDisappearing). Idempotent via -= then +=.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.CloseRequested -= OnClose; _vm.CloseRequested += OnClose;
        _vm.Saved -= OnSaved; _vm.Saved += OnSaved;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.CloseRequested -= OnClose;
        _vm.Saved -= OnSaved;
    }
}
