using Cali.Core.Services;
using Xunit;

public class ProfileMathTests
{
    [Theory]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    [InlineData("abc", 0)]
    [InlineData("-5", 0)]      // negatives clamp to min
    [InlineData(" 12 ", 12)]   // trims whitespace
    [InlineData("0", 0)]
    [InlineData("74", 74)]
    [InlineData("99999", 500)] // clamps to max
    public void ParseBounded_handles_user_input(string text, int expected)
        => Assert.Equal(expected, ProfileMath.ParseBounded(text, 0, 500));

    [Fact]
    public void ParseBounded_uses_min_for_invalid_and_clamps_low_values()
    {
        Assert.Equal(3, ProfileMath.ParseBounded("", 3, 10));   // invalid → min
        Assert.Equal(3, ProfileMath.ParseBounded("1", 3, 10));  // below min → min
        Assert.Equal(10, ProfileMath.ParseBounded("50", 3, 10)); // above max → max
    }
}
