using Microsoft.Maui.Graphics;

namespace Cali.Controls;

/// <summary>Accent rest ring. Draws a faint full track plus an accent arc for the remaining fraction.</summary>
public sealed class RingTimer : GraphicsView, IDrawable
{
    public static readonly BindableProperty FractionProperty = BindableProperty.Create(
        nameof(Fraction), typeof(double), typeof(RingTimer), 1.0,
        propertyChanged: (b, _, _) => ((RingTimer)b).Invalidate());

    public double Fraction
    {
        get => (double)GetValue(FractionProperty);
        set => SetValue(FractionProperty, value);
    }

    public RingTimer() => Drawable = this;

    public void Draw(ICanvas canvas, RectF rect)
    {
        const float stroke = 6f;
        float inset = stroke / 2 + 2;
        var r = new RectF(rect.X + inset, rect.Y + inset, rect.Width - 2 * inset, rect.Height - 2 * inset);

        // faint full track
        canvas.StrokeColor = Color.FromArgb("#262A33");
        canvas.StrokeSize = stroke;
        canvas.DrawEllipse(r);

        // accent arc for remaining fraction, starting at the top going clockwise
        var f = (float)Math.Clamp(Fraction, 0, 1);
        if (f > 0)
        {
            canvas.StrokeColor = Color.FromArgb("#FF6B1A");
            canvas.StrokeSize = stroke;
            float start = 90f;            // top
            float end = 90f - 360f * f;   // clockwise
            canvas.DrawArc(r.X, r.Y, r.Width, r.Height, start, end, true, false);
        }
    }
}
