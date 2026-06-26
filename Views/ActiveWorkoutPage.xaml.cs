using Cali.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;

namespace Cali.Views;

public partial class ActiveWorkoutPage : ContentPage
{
    private readonly ActiveWorkoutViewModel _vm;

    public ActiveWorkoutPage(ActiveWorkoutViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        try { DeviceDisplay.Current.KeepScreenOn = true; } catch { } // keep the screen awake mid-workout
        _vm.Finished -= OnWorkoutEnded; _vm.Finished += OnWorkoutEnded;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        try { DeviceDisplay.Current.KeepScreenOn = false; } catch { } // release when leaving the workout
        _vm.Finished -= OnWorkoutEnded;
        _vm.Detach();
    }

    private async void OnWorkoutEnded()
    {
        var planName = _vm.PlanName;
        await Navigation.PopModalAsync();

        var summary = IPlatformApplication.Current!.Services.GetRequiredService<SummaryPage>();
        await summary.PrepareAsync(planName);
        await Shell.Current.Navigation.PushModalAsync(summary);
    }
}
