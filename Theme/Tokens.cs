using Microsoft.Maui.Graphics;

namespace Cali;

/// <summary>Design tokens as <see cref="Color"/>s for code-side use (VMs, drawables).</summary>
public static class Tokens
{
    public static readonly Color Bg = Color.FromArgb("#0D0E11");
    public static readonly Color Surface = Color.FromArgb("#15171C");
    public static readonly Color Surface2 = Color.FromArgb("#1C1F26");
    public static readonly Color Line = Color.FromArgb("#262A33");
    public static readonly Color Text = Color.FromArgb("#F3F4F6");
    public static readonly Color TextMuted = Color.FromArgb("#9499A3");
    public static readonly Color TextFaint = Color.FromArgb("#888E99");
    public static readonly Color Accent = Color.FromArgb("#FF6B1A");
    public static readonly Color AccentTint = Color.FromArgb("#251710");
    public static readonly Color AccentOn = Color.FromArgb("#0D0E11");
    public static readonly Color Success = Color.FromArgb("#3ECF8E");
    public static readonly Color Danger = Color.FromArgb("#FF4D6B");
    public static readonly Color CustomBar = Color.FromArgb("#3A3F49");
    public static readonly Color Clear = Colors.Transparent;
}
