#  FoodDrinkApp - Smart Food Diary Application

A food diary application built with .NET MAUI for Android, 
demonstrating hardware integration and accessibility features.

## Project Overview

### Purpose

FoodDrinkApp is an educational demonstration project showcasing enterprise-level mobile application development using the .NET MAUI framework. The application integrates seven distinct mobile hardware features while maintaining strict accessibility standards, comprehensive error handling, and production-ready code quality.

### Target Audience

- iOS and Android users seeking a food tracking solution
- Developers studying mobile development best practices
- Teams evaluating .NET MAUI for cross-platform projects
- Accessibility advocates and inclusive design practitioners



### Project Structure

```
FoodDrinkApp/
├── Models/
│   └── FoodItem.cs
│       • Represents nutritional data for food/drink items
│       • JSON serialization compatible with REST APIs
│       • Computed properties for UI display
│
├── Services/
│   ├── FoodCatalogService.cs
│   │   • Manages CRUD operations for food items
│   │   • Implements API fallback to local cache
│   │   • Provides multi-field search capabilities
│   │
│   ├── AccessibilityService.cs
│   │   • Implements dynamic font scaling
│   │   • Manages theme switching (light/dark)
│   │   • Persistent preference storage
│   │
│   ├── SpeechService.cs
│   │   • Wraps TextToSpeech API with locale fallback
│   │   • Implements graceful error handling
│   │   • Supports cancellation tokens
│   │
│   └── MockApiConfig.cs
│       • Centralized API endpoint configuration
│       • Static properties for dependency-free access
│
├── Pages/ (XAML + Code-behind)
│   ├── MainPage.xaml/cs
│   │   • Food catalog display with CollectionView
│   │   • Real-time search with TextChanged event
│   │   • Pull-to-refresh integration
│   │
│   ├── AddItemPage.xaml/cs
│   │   • Form-based data entry with validation
│   │   • Allergy note specification
│   │   • Haptic feedback on validation errors
│   │
│   ├── FoodDetailPage.xaml/cs
│   │   • Route-based navigation with QueryProperty
│   │   • Read-aloud functionality via TTS
│   │   • Vibration feedback integration
│   │
│   ├── HardwarePage.xaml/cs
│   │   • Seven hardware feature demonstrations
│   │   • Real-time sensor data visualization
│   │   • User action counter for verification
│   │
│   ├── SettingsPage.xaml/cs
│   │   • Theme toggle with AppThemeBinding
│   │   • Large text mode configuration
│   │   • MockAPI endpoint configuration
│   │
│   └── AppShell.xaml/cs
│       • Route definitions
│       • Navigation hierarchy
│
├── Resources/
│   ├── Colors.xaml
│   │   • Semantic color definitions
│   │   • Light/dark theme variants
│   │   • WCAG contrast ratios verified
│   │
│   ├── Styles.xaml
│   │   • Global style definitions
│   │   • Typography hierarchy
│   │   • Consistent spacing and sizing
│   │
│   └── AppIcon/ & Splash/
│       • Platform-specific assets
│
├── Platforms/
│   └── Android/
│       • Platform-specific manifests
│       • Permission configurations
│
└── MauiProgram.cs
    • Dependency injection configuration
    • Service registration
    • Resource loading
```

---

## Feature Specification

### 1. Food Catalog Management

#### Search Functionality
- Multi-field search across name, category, description, and tags
- Case-insensitive comparison for user-friendly matching
- Real-time results update with minimal latency
- Empty query returns all items sorted alphabetically

#### Add Food Record
- Form-based UI with category picker
- Validation for all required fields
- Nutritional data entry (calories, protein, carbs, fat)
- Optional allergy note specification
- Automatic tag generation from input fields

#### Detail View
- Comprehensive nutritional breakdown
- Macronutrient summary and calorie information
- Allergy warnings and ingredient notes
- Text-to-speech readback functionality
- Stop reading control for playback management

#### Data Persistence
- Local in-memory cache for instant access
- Optional REST API integration via mockapi.io
- Automatic fallback to local data on API failure
- Maintains app usability in offline scenarios

### 2. Hardware Features

| Feature | Hardware API | Use Case | Fallback |
|---------|-------------|----------|----------|
| Camera | MediaPicker.Default | Capture meal photos | Display message "Camera not supported" |
| Location | Geolocation + Geocoding | Determine meal location | Fallback coordinates with estimated region |
| Speech | TextToSpeech.Default | Read nutrition information | Silent failure, no error dialog |
| Vibration | Vibration.Default | Provide tactile feedback | Hardware unavailable gracefully handled |
| Haptic Feedback | HapticFeedback.Default | Enhanced user interaction | Falls back to vibration if unavailable |
| Accelerometer | Accelerometer.Default | Detect device motion | Display "Not supported on this device" |
| Gyroscope | Gyroscope.Default | Measure device rotation | Display "Not supported on this device" |

### 3. Accessibility Features

#### WCAG 2.1 AA Compliance

**Visual Design**
- Minimum 4.5:1 contrast ratio for normal text
- Minimum 3:1 contrast ratio for large text (18pt+)
- No information conveyed by color alone
- Clear visual focus indicators

**Text Scalability**
- Large Text Mode multiplier: 1.22x
- Supports text sizes from 10pt to 30pt
- Settings persisted across app sessions
- Applied recursively to all UI elements

**Screen Reader Support**
- SemanticProperties on interactive elements
- AutomationId for UI element identification
- Semantic announcements for status updates
- HeadingLevel hierarchy for document structure

**Keyboard Navigation**
- Tab order follows logical reading order
- Enter/Space to activate buttons
- Arrow keys for list navigation
- Search bar accepts keyboard input

#### Theme System

**Light Theme**
- Background: Warm white (#FFFBF9)
- Primary text: Dark blue-gray (#0D3B52)
- Interactive: Brand teal (#0D7DBE)
- Accent: Light cyan (#C0E8F0)

**Dark Theme**
- Background: Deep navy (#0A1520)
- Primary text: Off-white (#E8F0F8)
- Interactive: Bright cyan (#7DD4E6)
- Accent: Deep teal (#1A5A7A)

**Toggle Mechanism**
- AppThemeBinding for automatic theme switching
- Preferences.Set("theme", value) for persistence
- Application.Current.UserAppTheme update on toggle
- No app restart required

---

## Hardware Integration

### 1. Camera Feature Implementation

```csharp
// Permission handling
if (!MediaPicker.Default.IsCaptureSupported)
    return ShowError("Device does not support camera capture");

// Capture and convert
var photo = await MediaPicker.Default.CapturePhotoAsync();
await using var stream = await photo.OpenReadAsync();
var memoryStream = new MemoryStream();
await stream.CopyToAsync(memoryStream);
var imageBytes = memoryStream.ToArray();

// Display
FoodPhoto.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));

// Error handling
catch (PermissionException)
    // Enable camera access in device settings
catch (Exception ex)
    // Log and display generic error
```

**Design Patterns**
- Resource disposal via `using` statements
- MemoryStream copy for stream safety
- PermissionException specific handling
- Haptic feedback on success (HapticFeedbackType.Click)

### 2. Location Feature Implementation

```csharp
// Geolocation request with timeout
var request = new GeolocationRequest(
    GeolocationAccuracy.Medium, 
    TimeSpan.FromSeconds(10));
var location = await Geolocation.Default.GetLocationAsync(request);

// Reverse geocoding to address
var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
var address = FormatPlacemark(placemarks?.FirstOrDefault());

// Fallback for emulator or unavailable services
if (IsNear(location, 37.422, -122.084, 0.08))
    return "United States / California / Mountain View";
```

**Advanced Features**
- Chinese-to-English address translation dictionary
- Coordinate-based fallback detection for common locations
- Tolerance-based location matching (±0.08 degrees)
- Medium accuracy for balance between speed and precision

### 3. Text-to-Speech Implementation

```csharp
// Locale detection with fallback
var locale = await FindEnglishLocaleAsync() ?? await FindAnyLocaleAsync();

// TTS with optional cancellation
var settings = new SpeechOptions
{
    Volume = 0.9f,
    Pitch = 1.05f,
    Locale = locale
};

await TextToSpeech.Default.SpeakAsync(text, settings, cancellationToken);

// Silent failure for device compatibility
catch (InvalidOperationException)
    // No error dialog - graceful degradation
```

**Robustness**
- English locale prioritized, any locale fallback available
- Cancellation token support for stop functionality
- Silent failure mode for incompatible devices (tested on OPPO)
- Volume and pitch customization for clarity

### 4. Vibration & Haptic Feedback Implementation

```csharp
// Haptic feedback for user interactions
HapticFeedback.Default.Perform(HapticFeedbackType.Click);        // Subtle feedback
HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);    // Strong feedback

// Timed vibration for notifications
Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(450));

// Duration options: 50ms, 250ms, 450ms, 1000ms+
```

**UX Enhancement**
- Click feedback on button presses
- Error feedback (250ms vibration)
- Success feedback (LongPress haptic)
- Fallback to standard vibration if haptic unavailable

### 5. Motion Sensors Implementation

#### Accelerometer (X, Y, Z acceleration in m/s²)

```csharp
var accelX = 0.0, accelY = 0.0, accelZ = 0.0;

var handler = new EventHandler<AccelerometerChangedEventArgs>((s, e) =>
{
    accelX = e.Reading.Acceleration.X;
    accelY = e.Reading.Acceleration.Y;
    accelZ = e.Reading.Acceleration.Z;
});

Accelerometer.Default.ReadingChanged += handler;
Accelerometer.Default.Start(SensorSpeed.UI);           // 200ms updates
await Task.Delay(500);                                 // Sample for 500ms
Accelerometer.Default.Stop();
Accelerometer.Default.ReadingChanged -= handler;
```

#### Gyroscope (X, Y, Z angular velocity in rad/s)

```csharp
var gyroX = 0.0, gyroY = 0.0, gyroZ = 0.0;

var handler = new EventHandler<GyroscopeChangedEventArgs>((s, e) =>
{
    gyroX = e.Reading.AngularVelocity.X;
    gyroY = e.Reading.AngularVelocity.Y;
    gyroZ = e.Reading.AngularVelocity.Z;
});

Gyroscope.Default.ReadingChanged += handler;
Gyroscope.Default.Start(SensorSpeed.UI);
await Task.Delay(500);
Gyroscope.Default.Stop();
Gyroscope.Default.ReadingChanged -= handler;
```

**Technical Details**
- SensorSpeed.UI: 200ms update interval (suitable for UI display)
- EventHandler pattern for continuous data streams
- Manual cleanup to prevent memory leaks
- Device support detection via IsSupported property

---

## Accessibility Features

### Font Scaling Implementation

```csharp
// Recursive traversal of visual tree
public static void ApplyFontScale(Element root)
{
    ApplyToElement(root);
    
    if (root is IVisualTreeElement visualTree)
    {
        foreach (var child in visualTree.GetVisualChildren().OfType<Element>())
            ApplyFontScale(child);
    }
}

// Individual element scaling
private static void ApplyToElement(Element element)
{
    var scale = LargeTextEnabled ? 1.22 : 1.0;
    
    switch (element)
    {
        case Label label:
            label.FontSize = GetOriginalFontSize(label, label.FontSize) * scale;
            break;
        case Button button:
            button.FontSize = GetOriginalFontSize(button, button.FontSize) * scale;
            break;
        // Additional element types...
    }
}
```

**Key Features**
- Weak reference table prevents memory leaks
- Original size preservation for toggle functionality
- All text-bearing elements supported
- Default 14pt fallback for unsized elements

### Theme Management

```csharp
// AppShell.xaml binding
<Shell.BackgroundColor>
    <AppThemeBinding Light="#FFFBF9" Dark="#0A1520" />
</Shell.BackgroundColor>

// Dynamic theme switching
Application.Current.UserAppTheme = AppTheme.Dark;  // or AppTheme.Light
Preferences.Set("theme", "dark");

// Persistence on app restart
var saved = Preferences.Get("theme", "light");
Application.Current.UserAppTheme = saved == "dark" ? AppTheme.Dark : AppTheme.Light;
```

---

## Technical Implementation

### Validation Framework

#### Form Validation Pipeline

```csharp
// Validation order: Required → Format → Logic
private string? ValidateForm(out int calories, out int protein, 
                             out int carbs, out int fat)
{
    // Step 1: Required fields
    if (string.IsNullOrWhiteSpace(NameEntry.Text))
        return "Please enter a food or drink name.";
    
    // Step 2: Format validation
    return TryReadNumber(CaloriesEntry.Text, "calories", out calories)
        ?? TryReadNumber(ProteinEntry.Text, "protein", out protein)
        ?? TryReadNumber(CarbsEntry.Text, "carbs", out carbs)
        ?? TryReadNumber(FatEntry.Text, "fat", out fat);
}

// Reusable numeric validation
private static string? TryReadNumber(string? value, 
                                     string fieldName, out int number)
{
    if (int.TryParse(value, out number) && number >= 0)
        return null;
    
    return $"Please enter a valid non-negative number for {fieldName}.";
}
```

#### Error Presentation

```csharp
// User-friendly error display
ValidationLabel.Text = errorMessage;
ValidationPanel.IsVisible = true;

// Accessibility announcement
SemanticScreenReader.Announce(errorMessage);

// Haptic feedback for errors
Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250));
```

### Error Handling Strategy

#### Exception Hierarchy

```
Exception
├── PermissionException          → "Enable [Feature] in device settings"
├── OperationCanceledException   → Silent handling (expected)
├── InvalidOperationException    → Detailed troubleshooting steps
├── HttpRequestException         → "Network unavailable, using local data"
└── Exception (generic)          → "An error occurred. Please try again."
```

#### Implementation Pattern

```csharp
try
{
    // Attempt operation with specific device API
    var result = await HardwareApi.GetDataAsync();
    DisplaySuccess(result);
}
catch (PermissionException ex)
{
    // Specific handling: user action required
    ShowPermissionDialog(ex.Message);
}
catch (OperationCanceledException)
{
    // Expected: user cancelled, silent handling
}
catch (InvalidOperationException ex)
{
    // Service-level error: detailed troubleshooting
    ShowDetailedError(ex);
}
catch (Exception ex)
{
    // Fallback: generic error message
    ShowGenericError(ex.Message);
}
```

### Async/Await Patterns

#### Rule 1: Non-Blocking I/O
```csharp
// ✅ Correct: UI thread not blocked
var data = await HttpClient.GetFromJsonAsync<List<FoodItem>>(url);

// ❌ Incorrect: Blocks UI thread
var data = HttpClient.GetFromJsonAsync<List<FoodItem>>(url).Result;
```

#### Rule 2: ConfigureAwait for Libraries
```csharp
// ✅ Library code: return to thread pool
var data = await service.GetDataAsync().ConfigureAwait(false);

// UI code: continue on UI thread (implicit)
var items = await service.GetDataAsync();
```

#### Rule 3: Proper Cancellation Support
```csharp
public async Task SpeakAsync(string text, CancellationToken token = default)
{
    try
    {
        await TextToSpeech.Default.SpeakAsync(text, settings, token);
    }
    catch (OperationCanceledException)
    {
        // Expected when Stop() called
    }
}
```

---

## Installation and Setup

### Prerequisites

**System Requirements**
- Windows 10/11 with Visual Studio 2022 or Mac with Visual Studio 2022 for Mac
- .NET 8.0 SDK or later
- Android SDK Platform API 21+ (Android 5.0 minimum)
- 4GB RAM minimum (8GB recommended)

**Software Dependencies**
```
.NET MAUI 8.0.0+
Android SDK
Java Development Kit (JDK) 11+
```

### Step-by-Step Installation

#### 1. Run on Android Emulator
```bash
# List available devices
dotnet run -f net8.0-android --device list

# Run on specific device
dotnet run -f net8.0-android -c Debug
```

#### 2. Deploy to Physical Device
```bash
# Connect device via USB with debugging enabled
dotnet run -f net8.0-android -c Release
```

### Common Issues and Solutions

#### 1. TTS Engine Not Found (OPPO Devices)

**Symptom**: "Failed to initialize Text to speech engine" error

**Root Cause**: System TTS engine not configured

**Solution**:
```
Settings → Accessibility → Text-to-speech output
→ Select engine (Google Text-to-Speech or system engine)
→ Verify via "Listen to an example" button
```

**Code Fallback**: Application gracefully silences TTS failures

#### 2. Location Permission Denied

**Symptom**: "Location permission was denied" message

**Root Cause**: User rejected location access

**Solution**:
```
Settings → Apps → FoodandDrinkApp → Permissions → Location
→ Select "Allow all the time" or "Allow only while using the app"
```

**Verification**: Test via HardwarePage → Locate button


#### 3. Emulator Camera Not Working

**Symptom**: "Camera not supported" error on emulator

**Root Cause**: Emulator not configured with virtual camera

**Solution**:
```
Android Virtual Device Manager → Edit Device
→ Camera: Front/Back = Emulated
→ Save and restart emulator
```

#### 4. Large Text Not Scaling

**Symptom**: Font size unchanged after toggling

**Root Cause**: Element not in AccessibilityService recursion

**Solution**:
- Verify element type is Label, Button, Entry, or Editor
- Check AppThemeBinding uses correct property names
- Manually test via SettingsPage toggle
- Inspect Visual Tree in Visual Studio

#### 5. Dark Theme Colors Inverted

**Symptom**: Unreadable text color in dark mode

**Root Cause**: Missing AppThemeBinding or incorrect color hex

**Solution**:
```xaml
<!-- Before: ❌ Color not theme-aware -->
<Label TextColor="#000000" />

<!-- After: ✅ Automatically switches by theme -->
<Label TextColor="{AppThemeBinding Light=#000000, Dark=#FFFFFF}" />
```

## Support and Documentation

- **Issue Tracker**: GitHub Issues
- **Code Documentation**: IntelliSense + XML docs
- **Technical Stack**: .NET 8.0, MAUI 8.0, Android SDK 21+

