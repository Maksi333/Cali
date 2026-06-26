using Cali.Core.Services;
using Cali.Tests.Fakes;
using Xunit;

public class SettingsServiceTests
{
    [Fact]
    public void Defaults_then_persists_round_trip()
    {
        var store = new InMemoryKeyValueStore();
        var svc = new SettingsService(store);

        Assert.True(svc.Settings.SoundAlerts);
        Assert.False(svc.Settings.AutoProgression);
        Assert.False(svc.IsOnboardingComplete);

        var s = svc.Settings;
        s.SoundAlerts = false; s.AutoProgression = true;
        svc.Save(s);
        svc.IsOnboardingComplete = true;
        svc.CurrentStreak = 12; svc.BestStreak = 21;

        var svc2 = new SettingsService(store); // re-read from same store
        Assert.False(svc2.Settings.SoundAlerts);
        Assert.True(svc2.Settings.AutoProgression);
        Assert.True(svc2.IsOnboardingComplete);
        Assert.Equal(12, svc2.CurrentStreak);
        Assert.Equal(21, svc2.BestStreak);
    }
}
