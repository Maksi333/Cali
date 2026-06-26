using System.Diagnostics;
using Cali.Core.Abstractions;
using Cali.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cali
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            UserAppTheme = AppTheme.Dark;
        }

        /// <summary>Completes when first-launch data init (DB tables, seed, achievement backfill) is done.
        /// Pages await this before querying so a fresh install can't race table creation.</summary>
        public static Task Initialization { get; private set; } = Task.CompletedTask;

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var services = activationState?.Context.Services;
            if (services is not null)
                Initialization = InitializeDataAsync(services);

            return new Window(new AppShell());
        }

        // First-launch: create tables, load the in-memory exercise library, seed presets/history/profile.
        private static async Task InitializeDataAsync(IServiceProvider services)
        {
            try
            {
                await services.GetRequiredService<IDatabase>().InitAsync();
                await services.GetRequiredService<ExerciseRepository>().InitAsync();
                await services.GetRequiredService<SeedService>().EnsureSeededAsync();
                // Silently backfill achievements already true from seed/history (no celebration).
                await services.GetRequiredService<AchievementService>().SyncAsync(DateTime.UtcNow, celebrate: false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Cali] Data init failed: {ex}");
            }
        }
    }
}
