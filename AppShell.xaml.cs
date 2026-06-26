using Cali.Core.Services;
using Cali.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Cali
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        // First-launch gate: show onboarding (full-screen modal) until completed.
        private async void OnLoaded(object? sender, EventArgs e)
        {
            Loaded -= OnLoaded;
            var services = IPlatformApplication.Current?.Services;
            if (services is null) return;

            var settings = services.GetRequiredService<SettingsService>();
            if (!settings.IsOnboardingComplete)
            {
                var onboarding = services.GetRequiredService<OnboardingPage>();
                await Navigation.PushModalAsync(onboarding, animated: false);
            }
        }
    }
}
