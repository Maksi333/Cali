using Cali.Core.Abstractions;

namespace Cali.Tests.Fakes;

public sealed class FileSeedDataProvider : ISeedDataProvider
{
    // Resolve the app's bundled seed (the single source of truth) by walking up from the test output directory.
    public static string SeedPath
    {
        get
        {
            var dir = AppContext.BaseDirectory;
            while (dir is not null &&
                   !File.Exists(Path.Combine(dir, "Resources", "Raw", "seed-data.json")))
            {
                dir = Directory.GetParent(dir)?.FullName;
            }
            if (dir is null)
                throw new FileNotFoundException("Resources/Raw/seed-data.json not found walking up from " + AppContext.BaseDirectory);
            return Path.Combine(dir, "Resources", "Raw", "seed-data.json");
        }
    }

    public Task<string> ReadSeedJsonAsync() => File.ReadAllTextAsync(SeedPath);
}
