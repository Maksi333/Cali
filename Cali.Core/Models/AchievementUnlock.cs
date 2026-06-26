using SQLite;

namespace Cali.Core.Models;

[Table("AchievementUnlocks")]
public sealed class AchievementUnlock
{
    [PrimaryKey] public string Id { get; set; } = "";
    public DateTime UnlockedUtc { get; set; }
}
