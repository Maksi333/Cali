using Microsoft.Maui.Graphics;

namespace Cali;

public static class TierColors
{
    public static Color Of(string tier) => tier switch
    {
        "bronze" => Color.FromArgb("#CD7F32"),
        "silver" => Color.FromArgb("#AEB4BE"),
        "gold" => Color.FromArgb("#FFC93C"),
        "platinum" => Color.FromArgb("#67E8F9"),
        "secret" => Color.FromArgb("#A77BF3"),
        _ => Tokens.TextMuted
    };
}
