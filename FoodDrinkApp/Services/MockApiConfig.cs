namespace FoodDrinkApp.Services;

public static class MockApiConfig
{
    // Replace this with the Resource endpoint.
    // Example: https://682xxxx.mockapi.io/api/v1/foods
    public const string EndpointUrl = "";

    public static bool IsConfigured => !string.IsNullOrWhiteSpace(EndpointUrl);
}
