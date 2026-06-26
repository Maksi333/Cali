namespace Cali.Core.Models;

public sealed class AppSettings
{
    public bool SoundAlerts { get; set; } = true;
    public bool Haptics { get; set; } = true;
    public bool AutoStartRest { get; set; } = true;
    public bool AutoProgression { get; set; }
    public bool Reminders { get; set; } = true;
    public string Units { get; set; } = "metric";
}
