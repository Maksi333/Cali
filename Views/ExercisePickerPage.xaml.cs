using Cali.Core.Models;
using Cali.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cali.Views;

public partial class ExercisePickerPage : ContentPage
{
    public Action<string>? OnPicked { get; set; }
    private readonly ExercisePickerViewModel _vm;

    public ExercisePickerPage()
    {
        InitializeComponent();
        _vm = IPlatformApplication.Current!.Services.GetRequiredService<ExercisePickerViewModel>();
        BindingContext = _vm;
        _vm.Load();
    }

    private async void OnSelect(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Exercise ex)
        {
            OnPicked?.Invoke(ex.Id);
            await Navigation.PopModalAsync();
        }
    }

    private async void OnClose(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
