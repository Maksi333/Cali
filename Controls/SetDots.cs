using Microsoft.Maui.Graphics;

namespace Cali.Controls;

/// <summary>Row of set dots: the first <see cref="Done"/> of <see cref="Total"/> are filled accent.</summary>
public sealed class SetDots : GraphicsView, IDrawable
{
    public static readonly BindableProperty TotalProperty = BindableProperty.Create(
        nameof(Total), typeof(int), typeof(SetDots), 0, propertyChanged: Redraw);

    public static readonly BindableProperty DoneProperty = BindableProperty.Create(
        nameof(Done), typeof(int), typeof(SetDots), 0, propertyChanged: Redraw);

    private static void Redraw(BindableObject b, object o, object n) => ((SetDots)b).Invalidate();

    public int Total { get => (int)GetValue(TotalProperty); set => SetValue(TotalProperty, value); }
    public int Done { get => (int)GetValue(DoneProperty); set => SetValue(DoneProperty, value); }

    public SetDots() => Drawable = this;

    public void Draw(ICanvas canvas, RectF rect)
    {
        if (Total <= 0) return;
        const float d = 34, gap = 12;
        float totalW = Total * d + (Total - 1) * gap;
        float x = rect.Right - totalW;              // right-align within the control
        float y = rect.Center.Y - d / 2;

        for (int i = 0; i < Total; i++)
        {
            var r = new RectF(x + i * (d + gap), y, d, d);
            if (i < Done)
            {
                canvas.FillColor = Color.FromArgb("#FF6B1A");
                canvas.FillRoundedRectangle(r, 10);
                canvas.FontColor = Color.FromArgb("#0D0E11");
            }
            else
            {
                canvas.StrokeColor = Color.FromArgb("#262A33");
                canvas.StrokeSize = 2;
                canvas.DrawRoundedRectangle(r, 10);
                canvas.FontColor = Color.FromArgb("#6B7079");
            }
            canvas.FontSize = 14;
            canvas.DrawString((i + 1).ToString(), r, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}
