using FoodDrinkApp.Services;

namespace FoodDrinkApp;

/// <summary>
/// Main page displaying food catalog with search and refresh capabilities.
/// </summary>
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Apply large text scaling if accessibility feature is enabled
        AccessibilityService.ApplyFontScale(this);
        // Load initial food data
        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    /// <summary>
    /// Loads and displays food items based on optional search query.
    /// </summary>
    private async Task LoadFoodItemsAsync(string? query = null)
    {
        FoodCollection.ItemsSource = await FoodCatalogService.SearchAsync(query);
    }

    /// <summary>
    /// Navigates to add item page when add button clicked.
    /// </summary>
    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddItemPage));
    }

    /// <summary>
    /// Navigates to detail page with selected food item ID.
    /// </summary>
    private async void OnDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            await Shell.Current.GoToAsync($"{nameof(FoodDetailPage)}?id={Uri.EscapeDataString(id)}");
        }
    }

    /// <summary>
    /// Searches as user types in search bar.
    /// </summary>
    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        await LoadFoodItemsAsync(e.NewTextValue);
    }

    /// <summary>
    /// Searches when search button pressed on keyboard.
    /// </summary>
    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    /// <summary>
    /// Pull-to-refresh handler to reload food data from catalog.
    /// </summary>
    private async void OnRefreshing(object? sender, EventArgs e)
    {
        // Reload food list
        await LoadFoodItemsAsync(SearchFoodBar.Text);
        FoodRefreshView.IsRefreshing = false;

        // Announce data source to screen reader users
        var source = FoodCatalogService.LastLoadUsedMockApi ? "mockapi.io" : "local fallback data";
        SemanticScreenReader.Announce($"Food and drink list refreshed. Current source: {source}.");
    }
}
