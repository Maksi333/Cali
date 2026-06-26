using Cali.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cali.Views;

public partial class OnboardingPage : ContentPage
{
    private readonly OnboardingViewModel _vm;

    public OnboardingPage()
    {
        InitializeComponent();
        _vm = IPlatformApplication.Current!.Services.GetRequiredService<OnboardingViewModel>();
        BindingContext = _vm;
        _vm.Completed += OnCompleted;
    }

    private async void OnCompleted()
    {
        _vm.Completed -= OnCompleted;
        await Navigation.PopModalAsync();
    }
}
