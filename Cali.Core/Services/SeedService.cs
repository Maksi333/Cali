using Cali.Core.Abstractions;
using Cali.Core.Models;

namespace Cali.Core.Services;

public sealed class SeedService
{
    private readonly ISeedDataProvider _seed;
    private readonly PlanRepository _plans;
    private readonly SettingsService _settings;
    private const string SeededKey = "cali.seeded";

    public SeedService(ISeedDataProvider seed, PlanRepository plans, SettingsService settings)
    {
        _seed = seed;
        _plans = plans;
        _settings = settings;
    }

    public async Task EnsureSeededAsync()
    {
        var data = SeedData.Parse(await _seed.ReadSeedJsonAsync());

        // Predefined plans are CONTENT, not user data — always seed them (idempotent).
        await _plans.SeedPresetsAsync(data.PredefinedPlans);

        // First launch must be a genuinely fresh state: no demo profile, no sample history,
        // no pre-unlocked achievements. We deliberately do NOT seed SampleProfile/SampleHistory.
        // Only sensible default settings are applied once.
        if (!_settings.IsKeySet(SeededKey))
        {
            if (data.DefaultSettings is { } s)
            {
                _settings.Save(new AppSettings
                {
                    SoundAlerts = s.SoundAlerts,
                    Haptics = s.Haptics,
                    AutoStartRest = s.AutoStartRest,
                    AutoProgression = s.AutoProgression,
                    Reminders = s.Reminders,
                    Units = s.Units
                });
            }
            _settings.MarkKey(SeededKey);
        }
    }
}
