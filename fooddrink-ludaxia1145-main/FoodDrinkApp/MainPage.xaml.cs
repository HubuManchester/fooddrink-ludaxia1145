using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    // Apply accessibility settings when page becomes visible
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    // Load food items from the catalog service with optional search filter
    private async Task LoadFoodItemsAsync(string? query = null)
    {
        FoodCollection.ItemsSource = await FoodCatalogService.SearchAsync(query);
    }

    // Navigate to the add page when "Add" button is clicked
    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddItemPage));
    }

    // Navigate to detail page for the selected food item
    private async void OnDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            await Shell.Current.GoToAsync($"{nameof(FoodDetailPage)}?id={Uri.EscapeDataString(id)}");
        }
    }

    // Real-time search as user types in the search bar
    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        await LoadFoodItemsAsync(e.NewTextValue);
    }

    // Execute search when user presses the search button
    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    // Refresh the food list and announce the data source
    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadFoodItemsAsync(SearchFoodBar.Text);
        FoodRefreshView.IsRefreshing = false;
        var source = FoodCatalogService.LastLoadUsedMockApi ? "mockapi.io" : "local fallback data";
        SemanticScreenReader.Announce($"Food and drink list refreshed. Current source: {source}.");
    }
}
