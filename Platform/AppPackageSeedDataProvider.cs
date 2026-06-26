using Cali.Core.Abstractions;
using Microsoft.Maui.Storage;

namespace Cali.Platform;

public sealed class AppPackageSeedDataProvider : ISeedDataProvider
{
    public async Task<string> ReadSeedJsonAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("seed-data.json");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
