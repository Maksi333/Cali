using System.Globalization;

namespace Cali.Core.Services;

/// <summary>Pure input helpers for editable profile fields. Kept in Core so they stay unit-testable
/// without a MAUI host.</summary>
public static class ProfileMath
{
    /// <summary>Parses user-entered text into an integer clamped to [min, max].
    /// Empty, non-numeric, or out-of-range input is handled gracefully (never throws):
    /// invalid text returns <paramref name="min"/>; valid numbers are clamped into range.</summary>
    public static int ParseBounded(string? text, int min, int max)
    {
        if (int.TryParse((text ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
            return Math.Clamp(v, min, max);
        return min;
    }
}
