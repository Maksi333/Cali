using Cali.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cali.Views;

public partial class SummaryPage : ContentPage
{
    private readonly SummaryViewModel _vm;

    public SummaryPage()
    {
        InitializeComponent();
        _vm = IPlatformApplication.Current!.Services.GetRequiredService<SummaryViewModel>();
        BindingContext = _vm;
    }

    public Task PrepareAsync(string planName) => _vm.PrepareAsync(planName);

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.CloseRequested -= OnClose; _vm.CloseRequested += OnClose;
        _ = AnimateInAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.CloseRequested -= OnClose;
    }

    private async void OnClose()
    {
        foreach (var t in _vm.NewlyUnlocked)
        {
            ToastEmoji.Text = t.Emoji;
            ToastName.Text = t.Name;
            ToastHost.Stroke = new SolidColorBrush(t.TierColor);
            ToastHost.Opacity = 0;
            ToastHost.Scale = 0.85;
            ToastHost.IsVisible = true;
            await Task.WhenAll(ToastHost.FadeTo(1, 200), ToastHost.ScaleTo(1, 300, Easing.SpringOut));
            await Task.Delay(1100);
            await ToastHost.FadeTo(0, 200);
            ToastHost.IsVisible = false;
        }

        await Navigation.PopModalAsync();
        await Shell.Current.GoToAsync("//progress");
    }

    private async Task AnimateInAsync()
    {
        CheckRoot.Scale = 0;
        PulseRing.Opacity = 0;
        await CheckRoot.ScaleTo(1, 450, Easing.SpringOut);
        PulseRing.Scale = 1;
        PulseRing.Opacity = 1;
        await Task.WhenAll(
            PulseRing.ScaleTo(1.3, 700, Easing.CubicOut),
            PulseRing.FadeTo(0, 700, Easing.CubicOut));
    }
}
