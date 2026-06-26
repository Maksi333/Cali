namespace Cali.Core.Services;

/// <summary>Consecutive-calendar-day streak from the set of days a workout was completed (local).</summary>
public static class StreakCalculator
{
    public static (int current, int best) Compute(IEnumerable<DateOnly> workoutDays, DateOnly today)
    {
        var days = workoutDays.Distinct().OrderBy(d => d).ToList();
        if (days.Count == 0) return (0, 0);

        int best = 1, run = 1;
        for (int i = 1; i < days.Count; i++)
        {
            if (days[i].DayNumber - days[i - 1].DayNumber == 1) { run++; best = Math.Max(best, run); }
            else run = 1;
        }

        // Current streak only counts if the most recent workout was today or yesterday.
        int current = 0;
        var last = days[^1];
        if (last == today || last == today.AddDays(-1))
        {
            current = 1;
            for (int i = days.Count - 2; i >= 0; i--)
            {
                if (days[i + 1].DayNumber - days[i].DayNumber == 1) current++;
                else break;
            }
        }
        return (current, best);
    }
}
