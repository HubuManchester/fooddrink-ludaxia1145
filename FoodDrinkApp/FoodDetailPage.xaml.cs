using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

/// <summary>
/// Displays detailed nutritional information for a selected food item.
/// Supports text-to-speech readback, vibration feedback, and accessibility features.
/// Route parameter: "id" - food item ID to display
/// </summary>
[QueryProperty(nameof(ItemId), "id")]
public partial class FoodDetailPage : ContentPage
{
    // Cached food item currently displayed on this page
    private FoodItem? currentItem;

    public FoodDetailPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Lifecycle: Called when page becomes visible.
    /// Applies accessibility font scaling based on user preferences.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Scale all text elements if large text mode is enabled
        AccessibilityService.ApplyFontScale(this);
    }

    /// <summary>
    /// Lifecycle: Called when page is closed or navigated away.
    /// Stops any active text-to-speech playback to prevent audio during navigation.
    /// </summary>
    protected override void OnDisappearing()
    {
        // Stop any ongoing speech to avoid playing audio while user navigates
        SpeechService.Stop();
        base.OnDisappearing();
    }

    /// <summary>
    /// Property bound from route parameter "id".
    /// Triggers async load of food item when set from navigation.
    /// </summary>
    public string ItemId
    {
        // Set ItemId from route parameter, immediately load corresponding food item
        set => _ = LoadItemAsync(value);
    }

    /// <summary>
    /// Loads food item from catalog by ID and renders on page.
    /// </summary>
    /// <param name="id">Unique identifier of food item to load</param>
    private async Task LoadItemAsync(string id)
    {
        // Retrieve food item from catalog service (API or local cache)
        currentItem = await FoodCatalogService.GetByIdAsync(id);
        // Set data context for UI binding
        BindingContext = currentItem;
        // Render item on page
        RenderItem();
    }

    /// <summary>
    /// Populates UI labels with food item data.
    /// Updates accessibility properties for screen readers.
    /// </summary>
    private void RenderItem()
    {
        // Handle case where item not found in catalog
        if (currentItem is null)
        {
            NameLabel.Text = "Record not found";
            DescriptionLabel.Text = "The selected food or drink could not be loaded.";
            return;
        }

        // Populate all UI labels with food item data
        NameLabel.Text = currentItem.Name;
        CategoryLabel.Text = currentItem.Category;
        CaloriesLabel.Text = currentItem.CaloriesLabel;
        MacroLabel.Text = currentItem.MacroSummary;
        DescriptionLabel.Text = currentItem.Description;
        AllergyLabel.Text = currentItem.AllergyNote;

        // Set accessible summary for screen readers (used by TTS)
        // This combines all nutritional info into one descriptive string
        SemanticProperties.SetDescription(NameLabel, currentItem.AccessibleSummary);
    }

    /// <summary>
    /// Reads nutrition information aloud using text-to-speech.
    /// Handler for "Read Summary" button click event.
    /// Silent failure: no error dialog if TTS unavailable.
    /// </summary>
    private async void OnSpeakClicked(object? sender, EventArgs e)
    {
        // Cannot read if no item loaded
        if (currentItem is null)
        {
            return;
        }

        try
        {
            // Speak the accessible summary (includes name, calories, macros, allergens)
            await SpeechService.SpeakAsync(currentItem.AccessibleSummary);
        }
        catch
        {
            // Silent failure: no error dialog on device without TTS
            // Allows app to work gracefully even on devices without speech engine
        }
    }

    /// <summary>
    /// Stops ongoing text-to-speech playback.
    /// Handler for "Stop Reading" button click event.
    /// Announces action to screen readers for accessibility.
    /// </summary>
    private void OnStopSpeechClicked(object? sender, EventArgs e)
    {
        // Stop active speech playback
        SpeechService.Stop();
        // Announce action to screen reader users
        SemanticScreenReader.Announce("Reading stopped.");
    }

    /// <summary>
    /// Triggers vibration and haptic feedback for demonstration and sensory confirmation.
    /// Handler for "Vibration Reminder" button click event.
    /// </summary>
    private async void OnVibrateClicked(object? sender, EventArgs e)
    {
        try
        {
            // 500ms vibration pulse for notification-style feedback
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
            // Haptic feedback: long press effect for enhanced tactile response
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            // Confirm action to user
            await DisplayAlert("Reminder", "Vibration feedback has been triggered.", "OK");
        }
        catch (Exception ex)
        {
            // Device does not support vibration or haptic feedback
            await DisplayAlert("Vibration unavailable", ex.Message, "OK");
        }
    }
}
