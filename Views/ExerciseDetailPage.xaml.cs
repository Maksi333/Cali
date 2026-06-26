using Cali.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cali.Views;

public partial class ExerciseDetailPage : ContentPage
{
    private readonly ExerciseDetailViewModel _vm;

    public ExerciseDetailPage()
    {
        InitializeComponent();
        _vm = IPlatformApplication.Current!.Services.GetRequiredService<ExerciseDetailViewModel>();
        BindingContext = _vm;
    }

    public void Load(string exerciseId) => _vm.Load(exerciseId);

    private async void OnBack(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
