namespace Cali.Core.Models;

/// <summary>An achievement with its live unlocked state + progress (for badges and the achievements grid).</summary>
public sealed class AchievementStatus
{
    public string Id { get; set; } = "";
    public string Emoji { get; set; } = "";
    public string Name { get; set; } = "";
    public string Desc { get; set; } = "";
    public string Category { get; set; } = "";
    public string Tier { get; set; } = "";
    public int Points { get; set; }
    public bool Unlocked { get; set; }
    public bool Hidden { get; set; }
    public int Cur { get; set; }
    public int Target { get; set; }
}
