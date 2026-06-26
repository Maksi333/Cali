using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Cali.ViewModels;

/// <summary>A selectable filter chip (muscle / difficulty).</summary>
public partial class ChipViewModel : ObservableObject
{
    public string Label { get; }
    public ChipViewModel(string label, bool selected = false)
    {
        Label = label;
        this.selected = selected;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BgColor))]
    [NotifyPropertyChangedFor(nameof(TextColor))]
    private bool selected;

    public Color BgColor => Selected ? Tokens.Accent : Tokens.Surface2;
    public Color TextColor => Selected ? Tokens.AccentOn : Tokens.TextMuted;
}
