namespace FoodDrinkApp.Services;

public static class SpeechService
{
    private static CancellationTokenSource? currentSpeech; // Token source to cancel ongoing speech

    // Main method to speak text asynchronously
    public static async Task SpeakAsync(string text)
    {
        Stop(); // Cancel any speech currently in progress

        currentSpeech = new CancellationTokenSource();
        var options = new SpeechOptions
        {
            Volume = 0.9f,
            Pitch = 1.05f,
            Locale = await FindEnglishLocaleAsync() ?? await FindAnyLocaleAsync() // Prefer English locale, fallback to any
        };

        try
        {
            await TextToSpeech.Default.SpeakAsync(text, options, currentSpeech.Token);
        }
        catch (OperationCanceledException)
        {
            // Speech was intentionally stopped; no action needed
        }
        catch (Exception ex) when (ex.Message.Contains("initialize", StringComparison.OrdinalIgnoreCase))
        {
            // Provide user-friendly guidance when TTS engine fails to initialize
            throw new InvalidOperationException(
                "TTS engine not properly configured on this device. Please:\n" +
                "1. Go to Settings > Accessibility > Text-to-speech output\n" +
                "2. Under 'Preferred engine', select an engine\n" +
                "3. If no engine listed, install Google Text-to-Speech from Play Store",
                ex);
        }
    }

    // Dedicated method for Chinese speech (simply calls SpeakAsync)
    public static Task SpeakChineseAsync(string text) => SpeakAsync(text);

    // Stops any currently active speech and cleans up resources
    public static void Stop()
    {
        if (currentSpeech is null)
        {
            return;
        }

        currentSpeech.Cancel(); // Signal cancellation
        currentSpeech.Dispose(); // Release resources
        currentSpeech = null;
    }

    // Attempts to find an English locale from available TTS locales
    private static async Task<Locale?> FindEnglishLocaleAsync()
    {
        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            return locales.FirstOrDefault(locale => locale.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return null; // Return null if locales cannot be retrieved
        }
    }

    // Fallback method: returns any available locale (first one found)
    private static async Task<Locale?> FindAnyLocaleAsync()
    {
        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            return locales.FirstOrDefault();
        }
        catch
        {
            return null; // Return null if locales cannot be retrieved
        }
    }
}