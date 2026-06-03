namespace FoodDrinkApp;

/// <summary>
/// Main application class for MealMate food diary app.
/// Initializes the application and creates the root navigation shell.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the application with XAML markup and resources.
    /// Called once when app first starts.
    /// </summary>
    public App()
    {
        // Load XAML definitions from App.xaml (styles, resources, etc.)
        InitializeComponent();
    }

    /// <summary>
    /// Creates the root window for the application.
    /// Sets up navigation shell and main UI container.
    /// </summary>
    /// <param name="activationState">Optional state passed during app activation (unused in this implementation)</param>
    /// <returns>Window containing the app shell and navigation structure</returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Create main window with AppShell as root navigation container
        // AppShell defines all routes: MainPage, FoodDetailPage, AddItemPage, etc.
        return new Window(new AppShell());
    }
}
