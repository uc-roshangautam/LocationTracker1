using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace LocationTracker1
{
    /// <summary>
    /// Entry point for configuring and building the MAUI application.
    /// Sets up services, fonts, and platform-specific features.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Creates and configures the MAUI application.
        /// </summary>
        /// <returns>The configured MauiApp instance.</returns>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiMaps() // Enable MAUI Maps component for location visualization
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            // Enable debug logging in development builds
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
