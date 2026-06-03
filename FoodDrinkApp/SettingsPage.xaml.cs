using FoodDrinkApp.Services;

namespace FoodDrinkApp;

/// <summary>
/// Settings page for accessibility and theme customization.
/// Provides controls for theme selection, font scaling, and API configuration.
/// </summary>
public partial class SettingsPage : ContentPage
{
    /// <summary>
    /// Initializes settings page and loads saved preferences.
    /// </summary>
    public SettingsPage()
    {
        InitializeComponent();
        // Set theme picker to system default (0 = Unspecified)
        ThemePicker.SelectedIndex = 0;
        // Load saved large text preference
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
    }

    /// <summary>
    /// Lifecycle: Called when settings page becomes visible.
    /// Refreshes UI to reflect current accessibility state.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Reload large text setting in case it was changed elsewhere
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
        // Update preview text and apply scaling
        ApplyLargeTextState();
    }

    /// <summary>
    /// Handler for theme picker selection change.
    /// Updates app theme immediately across all pages.
    /// </summary>
    private void OnThemeChanged(object? sender, EventArgs e)
    {
        // Apply selected theme based on picker index
        Application.Current!.UserAppTheme = ThemePicker.SelectedIndex switch
        {
            1 => AppTheme.Light,      // Light theme index
            2 => AppTheme.Dark,       // Dark theme index
            _ => AppTheme.Unspecified // System default
        };

        // Announce change to screen readers
        Announce("Theme preference updated.");
    }

    /// <summary>
    /// Handler for large text mode toggle.
    /// Enables/disables text scaling across entire application.
    /// </summary>
    private void OnLargeTextToggled(object? sender, ToggledEventArgs e)
    {
        // Update global accessibility setting
        AccessibilityService.LargeTextEnabled = e.Value;
        // Refresh UI with new font scale
        ApplyLargeTextState();
        // Announce state change to screen readers with descriptive message
        Announce(e.Value
            ? "Large text mode is on. Page text is now larger."
            : "Large text mode is off. Page text has returned to normal.");
    }

    /// <summary>
    /// Updates large text preview and applies font scaling to this page.
    /// Called after toggle change to immediately show new text size.
    /// </summary>
    private void ApplyLargeTextState()
    {
        // Recursively apply font scaling to all text elements on this page
        AccessibilityService.ApplyFontScale(this);

        // Update preview title based on current state
        LargeTextPreviewTitle.Text = AccessibilityService.LargeTextEnabled
            ? "Large text preview: enlarged"
            : "Large text preview";

        // Update preview description with guidance
        LargeTextPreviewBody.Text = AccessibilityService.LargeTextEnabled
            ? "Text is now noticeably larger. The food and hardware pages will use the same setting."
            : "Turn on the switch to enlarge this preview and other page text.";
    }

    /// <summary>
    /// Updates status label and announces message to screen readers.
    /// Used for confirming setting changes to all users.
    /// </summary>
    /// <param name="message">Message to display and announce</param>
    private void Announce(string message)
    {
        // Display message on page for sighted users
        SettingsStatusLabel.Text = message;
        // Announce message for screen reader users
        SemanticScreenReader.Announce(message);
    }
}
