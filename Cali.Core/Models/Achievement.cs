namespace Cali.Core.Models;

/// <summary>A static achievement definition (ships with the app). Unlock state is stored separately.</summary>
public sealed class Achievement
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Category { get; init; }   // milestones|consistency|volume|feats|skills|explorer|lifestyle|secret
    public required string Tier { get; init; }       // bronze|silver|gold|platinum|secret
    public required int Points { get; init; }
    public required string Emoji { get; init; }
    public bool Hidden { get; init; }
    public required Criteria Criteria { get; init; }
}
