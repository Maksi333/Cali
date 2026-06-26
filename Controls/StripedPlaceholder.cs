using Microsoft.Maui.Graphics;

namespace Cali.Controls;

/// <summary>Branded media placeholder used until real exercise illustrations exist:
/// the exercise's initial in the accent colour on a subtle accent-tinted tile.</summary>
public sealed class StripedPlaceholder : GraphicsView, IDrawable
{
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(StripedPlaceholder), "",
        propertyChanged: (b, _, _) => ((StripedPlaceholder)b).Invalidate());

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public StripedPlaceholder() => Drawable = this;

    public void Draw(ICanvas canvas, RectF rect)
    {
        // Base surface + faint accent-tinted wash (no literal "demo" text).
        canvas.FillColor = Color.FromArgb("#15171C");
        canvas.FillRectangle(rect);
        canvas.FillColor = Color.FromArgb("#251710"); // AccentTint
        canvas.FillRectangle(rect);

        var initial = string.IsNullOrWhiteSpace(Label) ? "•" : Label.Trim()[..1].ToUpperInvariant();
        bool large = rect.Height >= 90;

        canvas.FontColor = Color.FromArgb("#FF6B1A"); // Accent
        canvas.Font = new Microsoft.Maui.Graphics.Font("BarlowCondensedBold");
        canvas.FontSize = large ? rect.Height * 0.42f : rect.Height * 0.52f;
        canvas.DrawString(initial, rect, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}
