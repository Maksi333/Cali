using Cali.Core.Services;
using Cali.Tests.Fakes;
using Xunit;

public class SeedServiceTests
{
    private static (SeedService svc, PlanRepository plans, HistoryRepository hist, SettingsService settings, string path) Make()
    {
        var path = Path.Combine(Path.GetTempPath(), $"cali-{Guid.NewGuid():N}.db3");
        var db = new Database(path); db.InitAsync().GetAwaiter().GetResult();
        var plans = new PlanRepository(db);
        var hist = new HistoryRepository(db);
        var settings = new SettingsService(new InMemoryKeyValueStore());
        var seedProv = new FileSeedDataProvider();
        var svc = new SeedService(seedProv, plans, settings);
        return (svc, plans, hist, settings, path);
    }

    [Fact]
    public async Task First_run_is_fresh_state_and_second_run_is_noop()
    {
        var (svc, plans, hist, settings, path) = Make();
        try
        {
            await svc.EnsureSeededAsync();

            // Predefined plans are content and DO seed.
            Assert.Equal(6, (await plans.GetPresetsAsync()).Count);

            // A brand-new install must be completely fresh — no demo data of any kind.
            Assert.Empty(await hist.GetRecentAsync(50));      // no sample history
            Assert.Equal(0, settings.CurrentStreak);          // no seeded streak
            Assert.Equal(0, settings.BestStreak);
            Assert.False(settings.IsOnboardingComplete);      // seeding does NOT complete onboarding
            var p = settings.Profile;                         // no seeded "Alex" profile
            Assert.Equal("Athlete", p.Name);                  // neutral default, not demo data
            Assert.Equal(0, p.BodyweightKg);
            Assert.Equal(0, p.MaxPullups);

            // Default settings are applied once.
            Assert.True(settings.Settings.AutoStartRest);

            await svc.EnsureSeededAsync(); // idempotent
            Assert.Equal(6, (await plans.GetPresetsAsync()).Count);
            Assert.Empty(await hist.GetRecentAsync(50));
        }
        finally { try { File.Delete(path); } catch { } }
    }
}
