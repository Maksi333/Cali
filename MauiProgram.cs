using Cali.Core.Abstractions;
using Cali.Core.Services;
using Cali.Platform;
using Cali.ViewModels;
using Cali.Views;
using Microsoft.Extensions.Logging;

namespace Cali
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Barlow-Regular.ttf", "Barlow");
                    fonts.AddFont("Barlow-Medium.ttf", "BarlowMedium");
                    fonts.AddFont("Barlow-SemiBold.ttf", "BarlowSemiBold");
                    fonts.AddFont("Barlow-Bold.ttf", "BarlowBold");
                    fonts.AddFont("BarlowCondensed-SemiBold.ttf", "BarlowCondensedSemiBold");
                    fonts.AddFont("BarlowCondensed-Bold.ttf", "BarlowCondensedBold");
                });

            // Abstractions -> platform implementations
            builder.Services.AddSingleton<IKeyValueStore, PreferencesKeyValueStore>();
            builder.Services.AddSingleton<ISeedDataProvider, AppPackageSeedDataProvider>();
            builder.Services.AddTransient<ITicker>(_ => new DispatcherTicker(Application.Current!.Dispatcher));

            // Core services (singletons — shared app state)
            builder.Services.AddSingleton<IDatabase>(_ =>
                new Database(Path.Combine(FileSystem.AppDataDirectory, "cali.db3")));
            builder.Services.AddSingleton<SettingsService>();
            builder.Services.AddSingleton<ExerciseRepository>();
            builder.Services.AddSingleton<PlanRepository>();
            builder.Services.AddSingleton<HistoryRepository>();
            builder.Services.AddSingleton<PrRepository>();
            builder.Services.AddSingleton<SeedService>();
            builder.Services.AddSingleton<AchievementService>();

            // Active workout (hero)
            builder.Services.AddSingleton<IFeedbackService, AudioHapticService>();
            builder.Services.AddSingleton<WorkoutSessionService>();
            builder.Services.AddTransient<ActiveWorkoutViewModel>();
            builder.Services.AddTransient<ActiveWorkoutPage>();

            // Summary
            builder.Services.AddTransient<SummaryViewModel>();
            builder.Services.AddTransient<SummaryPage>();

            // Exercises library + detail
            builder.Services.AddTransient<ExercisesViewModel>();
            builder.Services.AddTransient<ExerciseDetailViewModel>();
            builder.Services.AddTransient<ExerciseDetailPage>();

            // Home / Progress / Profile / Onboarding + backup
            builder.Services.AddSingleton<BackupService>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<ProgressViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<OnboardingViewModel>();
            builder.Services.AddTransient<OnboardingPage>();

            // Achievements grid
            builder.Services.AddTransient<AchievementsViewModel>();
            builder.Services.AddTransient<AchievementsPage>();

            // Plans tab
            builder.Services.AddTransient<PlansViewModel>();

            // Plan builder + exercise picker
            builder.Services.AddTransient<PlanBuilderViewModel>();
            builder.Services.AddTransient<PlanBuilderPage>();
            builder.Services.AddTransient<ExercisePickerViewModel>();
            builder.Services.AddTransient<ExercisePickerPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
