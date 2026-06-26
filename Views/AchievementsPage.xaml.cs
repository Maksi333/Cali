using Cali.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cali.Views;

public partial class AchievementsPage : ContentPage
{
    private readonly AchievementsViewModel _vm;

    public AchievementsPage()
    {
        InitializeComponent();
        _vm = IPlatformApplication.Current!.Services.GetRequiredService<AchievementsViewModel>();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await App.Initialization;
        await _vm.LoadAsync();
    }

    private async void OnBack(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
