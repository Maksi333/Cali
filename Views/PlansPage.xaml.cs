using Cali.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cali.Views;

public partial class PlansPage : ContentPage
{
    private readonly PlansViewModel _vm;

    public PlansPage()
    {
        InitializeComponent();
        _vm = IPlatformApplication.Current!.Services.GetRequiredService<PlansViewModel>();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await App.Initialization;
        await _vm.LoadAsync();
    }
}
