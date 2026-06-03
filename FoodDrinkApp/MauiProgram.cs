using Microsoft.Extensions.Logging;

namespace FoodDrinkApp;

/// <summary>
/// Main entry point for MAUI application configuration and initialization.
/// Configures fonts, services, and platform-specific features.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the MAUI application with all necessary services and resources.
    /// </summary>
    /// <returns>Configured MauiApp instance ready for execution</returns>
    public static MauiApp CreateMauiApp()
    {
        // Initialize MAUI app builder
        var builder = MauiApp.CreateBuilder();

        // Configure the main app class and platform-specific settings
        builder
            .UseMauiApp<App>()
            // Load custom fonts for consistent typography across all platforms
            .ConfigureFonts(fonts =>
            {
                // OpenSans Regular: standard body text
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                // OpenSans Semibold: headings and emphasized text
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Enable debug logging only in DEBUG builds
        // This improves performance in Release builds by avoiding logging overhead
#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Build and return the configured application
        return builder.Build();
    }
}
