using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        ThemePicker.SelectedIndex = 0;
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
    }

    // Apply current large text setting when page appears
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
        ApplyLargeTextState();
    }

    // Change app theme preference (system, light, or dark)
    private void OnThemeChanged(object? sender, EventArgs e)
    {
        Application.Current!.UserAppTheme = ThemePicker.SelectedIndex switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

        Announce("Theme preference updated.");
    }

    // Toggle large text mode on/off and announce the change
    private void OnLargeTextToggled(object? sender, ToggledEventArgs e)
    {
        AccessibilityService.LargeTextEnabled = e.Value;
        ApplyLargeTextState();
        Announce(e.Value
            ? "Large text mode is on. Page text is now larger."
            : "Large text mode is off. Page text has returned to normal.");
    }

    // Apply or remove large text styling to page elements
    private void ApplyLargeTextState()
    {
        AccessibilityService.ApplyFontScale(this);

        LargeTextPreviewTitle.Text = AccessibilityService.LargeTextEnabled
            ? "Large text preview: enlarged"
            : "Large text preview";
        LargeTextPreviewBody.Text = AccessibilityService.LargeTextEnabled
            ? "Text is now noticeably larger. The food and hardware pages will use the same setting."
            : "Turn on the switch to enlarge this preview and other page text.";
    }

    // Update status label and announce to screen reader
    private void Announce(string message)
    {
        SettingsStatusLabel.Text = message;
        SemanticScreenReader.Announce(message);
    }
}
