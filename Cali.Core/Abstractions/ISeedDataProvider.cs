namespace Cali.Core.Abstractions;

public interface ISeedDataProvider
{
    Task<string> ReadSeedJsonAsync();
}
