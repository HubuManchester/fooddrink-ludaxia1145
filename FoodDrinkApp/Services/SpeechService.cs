namespace FoodDrinkApp.Services;

/// <summary>
/// Wraps MAUI TextToSpeech API with locale detection, error handling, and cancellation support.
/// Provides text-to-speech functionality with graceful fallback and detailed error messages.
/// </summary>
public static class SpeechService
{
    // Tracks current speech operation for cancellation when Stop() is called
    private static CancellationTokenSource? currentSpeech;

    /// <summary>
    /// Speaks text aloud with English locale preference and graceful error handling.
    /// Stops any existing speech before starting new speech.
    /// </summary>
    /// <param name="text">Text content to read aloud</param>
    /// <exception cref="InvalidOperationException">Thrown if TTS engine not properly configured on device</exception>
    public static async Task SpeakAsync(string text)
    {
        // Cancel any existing speech operation
        Stop();

        // Initialize new cancellation token for this speech operation
        currentSpeech = new CancellationTokenSource();

        // Configure speech settings: volume 0.9 (90%), pitch 1.05 (slightly higher for clarity)
        var options = new SpeechOptions
        {
            Volume = 0.9f,
            Pitch = 1.05f,
            // Prioritize English locale, fallback to any available locale
            Locale = await FindEnglishLocaleAsync() ?? await FindAnyLocaleAsync()
        };

        try
        {
            // Perform text-to-speech with cancellation token support
            await TextToSpeech.Default.SpeakAsync(text, options, currentSpeech.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when user calls Stop() - silent handling
        }
        catch (Exception ex) when (ex.Message.Contains("initialize", StringComparison.OrdinalIgnoreCase))
        {
            // TTS engine not initialized - provide detailed troubleshooting steps
            throw new InvalidOperationException(
                "TTS engine not properly configured on this device. Please:\n" +
                "1. Go to Settings > Accessibility > Text-to-speech output\n" +
                "2. Under 'Preferred engine', select an engine\n" +
                "3. If no engine listed, install Google Text-to-Speech from Play Store",
                ex);
        }
    }

    /// <summary>
    /// Alias for SpeakAsync - currently speaks English text (can be extended for Chinese in future).
    /// </summary>
    public static Task SpeakChineseAsync(string text) => SpeakAsync(text);

    /// <summary>
    /// Stops current speech playback and cleans up resources.
    /// Safe to call even if no speech is currently playing.
    /// </summary>
    public static void Stop()
    {
        // No active speech operation
        if (currentSpeech is null)
        {
            return;
        }

        // Cancel the ongoing speech operation
        currentSpeech.Cancel();
        // Dispose resources to prevent memory leaks
        currentSpeech.Dispose();
        // Clear reference for next operation
        currentSpeech = null;
    }

    /// <summary>
    /// Finds English locale from available text-to-speech engines.
    /// Returns first English locale found, prioritized for user experience.
    /// </summary>
    /// <returns>English Locale if found, null otherwise</returns>
    private static async Task<Locale?> FindEnglishLocaleAsync()
    {
        try
        {
            // Get all available TTS locales on device
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            // Return first locale starting with "en" (English, English-US, etc.)
            return locales.FirstOrDefault(locale => locale.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            // Locale detection failed - return null for fallback
            return null;
        }
    }

    /// <summary>
    /// Finds any available text-to-speech locale as fallback.
    /// Used when English locale is not available on device.
    /// </summary>
    /// <returns>Any available Locale, null if none available</returns>
    private static async Task<Locale?> FindAnyLocaleAsync()
    {
        try
        {
            // Get all available TTS locales on device
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            // Return first available locale regardless of language
            return locales.FirstOrDefault();
        }
        catch
        {
            // Locale detection failed - no TTS available
            return null;
        }
    }
}

