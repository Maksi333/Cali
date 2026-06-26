using Microsoft.Maui.Graphics;

namespace Cali.Controls;

/// <summary>Weekly volume bar chart: 7 bars, one highlighted (accent), the rest muted, with day labels.</summary>
public sealed class VolumeBarChart : GraphicsView, IDrawable
{
    public static readonly BindableProperty ValuesProperty = BindableProperty.Create(
        nameof(Values), typeof(double[]), typeof(VolumeBarChart), new double[7], propertyChanged: Redraw);
    public static readonly BindableProperty LabelsProperty = BindableProperty.Create(
        nameof(Labels), typeof(string[]), typeof(VolumeBarChart), new string[7], propertyChanged: Redraw);
    public static readonly BindableProperty HighlightProperty = BindableProperty.Create(
        nameof(Highlight), typeof(int), typeof(VolumeBarChart), 6, propertyChanged: Redraw);

    private static void Redraw(BindableObject b, object o, object n) => ((VolumeBarChart)b).Invalidate();

    public double[] Values { get => (double[])GetValue(ValuesProperty); set => SetValue(ValuesProperty, value); }
    public string[] Labels { get => (string[])GetValue(LabelsProperty); set => SetValue(LabelsProperty, value); }
    public int Highlight { get => (int)GetValue(HighlightProperty); set => SetValue(HighlightProperty, value); }

    public VolumeBarChart() => Drawable = this;

    public void Draw(ICanvas canvas, RectF rect)
    {
        var vals = Values ?? Array.Empty<double>();
        if (vals.Length == 0) return;
        float labelH = 22;
        float chartH = rect.Height - labelH;
        float max = (float)Math.Max(1, vals.Length == 0 ? 1 : vals.Max());
        float slot = rect.Width / vals.Length;
        float barW = Math.Min(slot * 0.5f, 40);

        for (int i = 0; i < vals.Length; i++)
        {
            float h = Math.Max(8, (float)(vals[i] / max) * (chartH - 12));
            float cx = rect.X + slot * i + slot / 2;
            var bar = new RectF(cx - barW / 2, rect.Y + chartH - h, barW, h);
            canvas.FillColor = i == Highlight ? Color.FromArgb("#FF6B1A") : Color.FromArgb("#2D323B");
            canvas.FillRoundedRectangle(bar, 6);

            if (Labels is { Length: > 0 } && i < Labels.Length)
            {
                canvas.FontColor = Color.FromArgb("#6B7079");
                canvas.FontSize = 11;
                canvas.DrawString(Labels[i], rect.X + slot * i, rect.Y + chartH + 2, slot, labelH,
                    HorizontalAlignment.Center, VerticalAlignment.Center);
            }
        }
    }
}
