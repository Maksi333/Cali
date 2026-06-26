namespace Cali.Core.Models;

public sealed class Profile
{
    public string Name { get; set; } = "Athlete";
    public string Goal { get; set; } = "Strength";
    public string Level { get; set; } = "Beginner";
    public int BodyweightKg { get; set; }
    public int MaxPullups { get; set; }
}
