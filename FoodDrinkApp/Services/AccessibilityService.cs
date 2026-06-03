using System.Runtime.CompilerServices;

namespace FoodDrinkApp.Services;

/// <summary>
/// Provides accessibility features including font scaling and theme support.
/// </summary>
public static class AccessibilityService
{
    private const double LargeTextScale = 1.22;
    private static readonly ConditionalWeakTable<BindableObject, FontSizeStore> OriginalFontSizes = new();

    /// <summary>
    /// Gets or sets large text mode enabled state.
    /// </summary>
    public static bool LargeTextEnabled { get; set; }

    /// <summary>
    /// Recursively applies font scaling to all text elements on the page.
    /// </summary>
    public static void ApplyFontScale(Element root)
    {
        ApplyToElement(root);

        if (root is not IVisualTreeElement visualTreeElement)
        {
            return;
        }

        // Process all child elements recursively
        foreach (var child in visualTreeElement.GetVisualChildren().OfType<Element>())
        {
            ApplyFontScale(child);
        }
    }

    /// <summary>
    /// Applies font size scaling to individual text-bearing element.
    /// </summary>
    private static void ApplyToElement(Element element)
    {
        var scale = LargeTextEnabled ? LargeTextScale : 1.0;

        // Scale font size for common text elements
        switch (element)
        {
            case Label label:
                label.FontSize = GetOriginalFontSize(label, label.FontSize) * scale;
                break;
            case Button button:
                button.FontSize = GetOriginalFontSize(button, button.FontSize) * scale;
                break;
            case Entry entry:
                entry.FontSize = GetOriginalFontSize(entry, entry.FontSize) * scale;
                break;
            case Editor editor:
                editor.FontSize = GetOriginalFontSize(editor, editor.FontSize) * scale;
                break;
            case Picker picker:
                picker.FontSize = GetOriginalFontSize(picker, picker.FontSize) * scale;
                break;
            case SearchBar searchBar:
                searchBar.FontSize = GetOriginalFontSize(searchBar, searchBar.FontSize) * scale;
                break;
        }
    }

    /// <summary>
    /// Retrieves original font size for scaling calculations. Stores it for future toggles.
    /// </summary>
    private static double GetOriginalFontSize(BindableObject control, double currentSize)
    {
        var store = OriginalFontSizes.GetOrCreateValue(control);
        if (!store.HasValue)
        {
            // Default to 14 if size not set
            store.Value = currentSize > 0 ? currentSize : 14;
            store.HasValue = true;
        }

        return store.Value;
    }

    /// <summary>
    /// Internal storage for original font size values using weak reference.
    /// </summary>
    private sealed class FontSizeStore
    {
        public bool HasValue { get; set; }
        public double Value { get; set; }
    }
}
