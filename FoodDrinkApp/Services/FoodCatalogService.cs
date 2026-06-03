using System.Net.Http.Json;
using System.Text.Json;
using FoodDrinkApp.Models;

namespace FoodDrinkApp.Services;

/// <summary>
/// Manages food/drink data from REST API or local fallback.
/// Provides search, retrieval, and addition of food items.
/// </summary>
public static class FoodCatalogService
{
    // HTTP client with 12-second timeout for API calls
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(12)
    };

    // JSON serialization options: case-insensitive for API compatibility
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Default food items used when API is unavailable.
    /// </summary>
    private static readonly List<FoodItem> LocalFallbackItems =
    [
        new()
        {
            Name = "Blueberry Smoothie Bowl",
            Category = "Breakfast",
            Description = "Açai base topped with granola, fresh blueberries, coconut flakes, and almond butter.",
            Calories = 385,
            Protein = 12,
            Carbs = 48,
            Fat = 18,
            AllergyNote = "Contains tree nuts and coconut.",
            Tags = "breakfast smoothie acai bowl antioxidants"
        },
        new()
        {
            Name = "Grilled Salmon Plate",
            Category = "Lunch",
            Description = "Pan-seared salmon fillet with roasted sweet potato, broccoli, and lime-butter sauce.",
            Calories = 580,
            Protein = 42,
            Carbs = 35,
            Fat = 28,
            AllergyNote = "Contains fish.",
            Tags = "lunch protein salmon omega3 meal prep"
        },
        new()
        {
            Name = "Green Tea with Honey",
            Category = "Drink",
            Description = "Fresh brewed green tea served warm with a touch of raw honey and lemon.",
            Calories = 45,
            Protein = 0,
            Carbs = 12,
            Fat = 0,
            AllergyNote = "No common allergens.",
            Tags = "drink tea healthy antioxidants caffeine"
        },
        new()
        {
            Name = "Quinoa Buddha Bowl",
            Category = "Dinner",
            Description = "Protein-rich quinoa with chickpeas, roasted beets, kale, tahini dressing, and pomegranate seeds.",
            Calories = 520,
            Protein = 18,
            Carbs = 62,
            Fat = 20,
            AllergyNote = "Contains sesame (tahini).",
            Tags = "vegetarian dinner vegan quinoa protein healthy"
        }
    ];

    /// <summary>
    /// Cached items from last successful API call or local fallback.
    /// </summary>
    private static List<FoodItem> cachedItems = new(LocalFallbackItems);

    /// <summary>
    /// Indicates whether the last data load used the MockAPI or fell back to local data.
    /// </summary>
    public static bool LastLoadUsedMockApi { get; private set; }

    /// <summary>
    /// Searches food items by query string across name, category, description, and tags.
    /// </summary>
    public static async Task<IReadOnlyList<FoodItem>> SearchAsync(string? query)
    {
        var items = await GetAllAsync();

        // Return all items sorted if no query
        if (string.IsNullOrWhiteSpace(query))
        {
            return items.OrderBy(item => item.Name).ToList();
        }

        // Multi-field case-insensitive search
        var normalised = query.Trim();
        return items
            .Where(item =>
                item.Name.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                item.Category.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                item.Description.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                item.Tags.Contains(normalised, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Name)
            .ToList();
    }

    /// <summary>
    /// Retrieves a single food item by ID from cache or API.
    /// </summary>
    public static async Task<FoodItem?> GetByIdAsync(string id)
    {
        // Try API first if configured
        if (MockApiConfig.IsConfigured)
        {
            try
            {
                var item = await HttpClient.GetFromJsonAsync<FoodItem>(
                    $"{MockApiConfig.EndpointUrl.TrimEnd('/')}/{Uri.EscapeDataString(id)}",
                    JsonOptions);

                if (item is not null)
                {
                    return item;
                }
            }
            catch
            {
                // Fall back to cache if API fails
            }
        }

        // Search in cached items
        return cachedItems.FirstOrDefault(item => item.Id == id);
    }

    /// <summary>
    /// Adds a new food item to the catalog and updates cache.
    /// </summary>
    public static async Task<FoodItem> AddAsync(FoodItem item)
    {
        // Post to API if configured
        if (MockApiConfig.IsConfigured)
        {
            var response = await HttpClient.PostAsJsonAsync(MockApiConfig.EndpointUrl, item, JsonOptions);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<FoodItem>(JsonOptions);
            if (created is not null)
            {
                cachedItems.Add(created);
                return created;
            }
        }

        // Add to local cache if no API or API failed
        cachedItems.Add(item);
        return item;
    }

    /// <summary>
    /// Retrieves all food items from API or returns cached items if unavailable.
    /// </summary>
    private static async Task<IReadOnlyList<FoodItem>> GetAllAsync()
    {
        // No API configured - return cache
        if (!MockApiConfig.IsConfigured)
        {
            LastLoadUsedMockApi = false;
            return cachedItems;
        }

        try
        {
            var items = await HttpClient.GetFromJsonAsync<List<FoodItem>>(MockApiConfig.EndpointUrl, JsonOptions);
            if (items is { Count: > 0 })
            {
                cachedItems = items;
                LastLoadUsedMockApi = true;
                return cachedItems;
            }
        }
        catch
        {
            // Keep the app usable during demos even if network unavailable
        }

        // Fall back to cached items
        LastLoadUsedMockApi = false;
        return cachedItems;
    }
}
