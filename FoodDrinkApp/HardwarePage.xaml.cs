using FoodDrinkApp.Services;

namespace FoodDrinkApp;

/// <summary>
/// Demonstrates seven mobile hardware features: camera, location, text-to-speech,
/// vibration, haptic feedback, accelerometer, and gyroscope.
/// </summary>
public partial class HardwarePage : ContentPage
{
    private int feedbackTestCount;

    public HardwarePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Apply large text scaling if accessibility feature is enabled
        AccessibilityService.ApplyFontScale(this);
    }

    protected override void OnDisappearing()
    {
        // Ensure text-to-speech is stopped when leaving the page
        SpeechService.Stop();
        base.OnDisappearing();
    }

    /// <summary>
    /// Captures a photo from the device camera and displays it.
    /// </summary>
    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                SetStatus("This device does not support camera capture.");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null)
            {
                SetStatus("Photo capture cancelled.");
                return;
            }

            // Convert captured photo to MemoryStream for display
            await using var stream = await photo.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();
            FoodPhoto.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            SetStatus("Food photo captured successfully.");
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (PermissionException)
        {
            SetStatus("Camera permission was denied. Enable camera access in device settings.");
        }
        catch (Exception ex)
        {
            SetStatus($"Camera error: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets device location and reverses geocodes to address format.
    /// </summary>
    private async void OnGetLocationClicked(object? sender, EventArgs e)
    {
        try
        {
            SetStatus("Getting location...");
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location is null)
            {
                SetStatus("Current location could not be found.");
                return;
            }

            // Display latitude and longitude with precision
            CoordinateLabel.Text = $"Latitude {location.Latitude:F5}, longitude {location.Longitude:F5}";
            LocationLabel.Text = await BuildAddressTextAsync(location);
            SetStatus("Country, city, and coordinates have been loaded.");
        }
        catch (PermissionException)
        {
            SetStatus("Location permission was denied. Enable location access in device settings.");
        }
        catch (Exception ex)
        {
            SetStatus($"Location error: {ex.Message}");
        }
    }

    /// <summary>
    /// Reverse geocodes GPS coordinates to human-readable address.
    /// </summary>
    private static async Task<string> BuildAddressTextAsync(Location location)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();
            var address = FormatPlacemark(placemark);

            if (!string.IsNullOrWhiteSpace(address))
            {
                return address;
            }
        }
        catch
        {
            // Fall back to coordinate-based address if geocoding fails
        }

        return BuildFallbackAddress(location);
    }

    /// <summary>
    /// Formats placemark data with Chinese-to-English translation for international support.
    /// </summary>
    private static string FormatPlacemark(Placemark? placemark)
    {
        if (placemark is null)
        {
            return string.Empty;
        }

        // Translation map for common Chinese location names
        var chineseToEnglish = new Dictionary<string, string>
        {
            { "中国", "China" },
            { "北京市", "Beijing" },
            { "上海市", "Shanghai" },
            { "广东省", "Guangdong" },
            { "浙江省", "Zhejiang" },
            { "江苏省", "Jiangsu" },
            { "四川省", "Sichuan" },
            { "湖北省", "Hubei" },
            { "湖南省", "Hunan" },
            { "安徽省", "Anhui" },
            { "江西省", "Jiangxi" },
            { "山东省", "Shandong" },
            { "河南省", "Henan" },
            { "河北省", "Hebei" },
            { "山西省", "Shanxi" },
            { "陕西省", "Shaanxi" },
            { "甘肃省", "Gansu" },
            { "青海省", "Qinghai" },
            { "新疆维吾尔自治区", "Xinjiang" },
            { "西藏自治区", "Tibet" },
            { "宁夏回族自治区", "Ningxia" },
            { "广西壮族自治区", "Guangxi" },
            { "内蒙古自治区", "Inner Mongolia" },
            { "北京", "Beijing" },
            { "上海", "Shanghai" },
            { "天津", "Tianjin" },
            { "重庆", "Chongqing" },
            { "武汉市", "Wuhan" },
            { "武昌区", "Wuchang" },
            { "汉口区", "Hankou" },
            { "汉阳区", "Hanyang" }
        };

        // Collect and translate address components
        var parts = new[]
        {
            placemark.CountryName,
            placemark.AdminArea,
            placemark.Locality,
            placemark.SubLocality
        }
        .Where(part => !string.IsNullOrWhiteSpace(part))
        .Distinct()
        .Select(part => chineseToEnglish.ContainsKey(part) ? chineseToEnglish[part] : part)
        .ToArray();

        return parts.Length == 0 ? string.Empty : string.Join(" / ", parts);
    }

    /// <summary>
    /// Provides fallback address for emulator or when geocoding service unavailable.
    /// </summary>
    private static string BuildFallbackAddress(Location location)
    {
        // Known location: Mountain View (Google HQ)
        if (IsNear(location, 37.422, -122.084, 0.08))
        {
            return "United States / California / Mountain View";
        }

        // San Francisco Bay Area coordinates range
        if (location.Latitude is >= 37.0 and <= 38.2 && location.Longitude is >= -123.2 and <= -121.5)
        {
            return "United States / California / San Francisco Bay Area";
        }

        // China coordinates range
        if (location.Latitude is >= 18 and <= 54 && location.Longitude is >= 73 and <= 135)
        {
            return "China / Current city requires a real device or available geocoding service";
        }

        return "Coordinates were found, but country and city were not returned by this device.";
    }

    /// <summary>
    /// Checks if location is within tolerance range of target coordinates.
    /// </summary>
    private static bool IsNear(Location location, double latitude, double longitude, double tolerance)
    {
        return Math.Abs(location.Latitude - latitude) <= tolerance &&
               Math.Abs(location.Longitude - longitude) <= tolerance;
    }

    /// <summary>
    /// Reads help text aloud using text-to-speech engine.
    /// </summary>
    private async void OnReadHelpClicked(object? sender, EventArgs e)
    {
        try
        {
            const string helpText = "MealMate records foods and drinks, shows nutrition details, and uses camera, location, speech, and haptic feedback to make meal tracking more practical.";
            await SpeechService.SpeakAsync(helpText);
            SetStatus("Reading help content aloud.");
        }
        catch (InvalidOperationException ex)
        {
            SetStatus(ex.Message);
        }
        catch (Exception ex)
        {
            SetStatus($"Text to speech error: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops active text-to-speech playback.
    /// </summary>
    private void OnStopSpeechClicked(object? sender, EventArgs e)
    {
        SpeechService.Stop();
        SetStatus("Reading stopped.");
    }

    /// <summary>
    /// Triggers vibration and haptic feedback for testing and demonstrates counter increment.
    /// </summary>
    private void OnFeedbackClicked(object? sender, EventArgs e)
    {
        try
        {
            // Vibration: 450ms pulse
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(450));
            // Haptic feedback: Long press impact effect
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);

            feedbackTestCount++;
            FeedbackCountLabel.Text = $"Haptic feedback tests: {feedbackTestCount}";
            SetStatus("Vibration and haptic feedback triggered. The changing counter can be used for screen-recorded verification.");
        }
        catch (Exception ex)
        {
            SetStatus($"Feedback error: {ex.Message}");
        }
    }

    /// <summary>
    /// Measures and displays accelerometer and gyroscope data for 500ms each.
    /// </summary>
    private async void OnMeasureMotionClicked(object? sender, EventArgs e)
    {
        try
        {
            SetStatus("Measuring motion sensors...");

            // Read accelerometer data (X, Y, Z acceleration in m/s²)
            if (Accelerometer.Default.IsSupported)
            {
                var accelX = 0.0;
                var accelY = 0.0;
                var accelZ = 0.0;

                var accelHandler = new EventHandler<AccelerometerChangedEventArgs>((s, e) =>
                {
                    accelX = e.Reading.Acceleration.X;
                    accelY = e.Reading.Acceleration.Y;
                    accelZ = e.Reading.Acceleration.Z;
                });

                Accelerometer.Default.ReadingChanged += accelHandler;
                Accelerometer.Default.Start(SensorSpeed.UI);
                await Task.Delay(500);  // Collect for 500ms
                Accelerometer.Default.Stop();
                Accelerometer.Default.ReadingChanged -= accelHandler;

                AccelerometerLabel.Text = $"Accelerometer: X={accelX:F2} Y={accelY:F2} Z={accelZ:F2}";
            }
            else
            {
                AccelerometerLabel.Text = "Accelerometer: Not supported on this device";
            }

            // Read gyroscope data (X, Y, Z angular velocity in rad/s)
            if (Gyroscope.Default.IsSupported)
            {
                var gyroX = 0.0;
                var gyroY = 0.0;
                var gyroZ = 0.0;

                var gyroHandler = new EventHandler<GyroscopeChangedEventArgs>((s, e) =>
                {
                    gyroX = e.Reading.AngularVelocity.X;
                    gyroY = e.Reading.AngularVelocity.Y;
                    gyroZ = e.Reading.AngularVelocity.Z;
                });

                Gyroscope.Default.ReadingChanged += gyroHandler;
                Gyroscope.Default.Start(SensorSpeed.UI);
                await Task.Delay(500);  // Collect for 500ms
                Gyroscope.Default.Stop();
                Gyroscope.Default.ReadingChanged -= gyroHandler;

                GyroscopeLabel.Text = $"Gyroscope: X={gyroX:F2} Y={gyroY:F2} Z={gyroZ:F2}";
            }
            else
            {
                GyroscopeLabel.Text = "Gyroscope: Not supported on this device";
            }

            MotionStatusLabel.Text = "Sensor data captured. Try moving your device and clicking again.";
            SetStatus("Motion sensors measured successfully.");
        }
        catch (Exception ex)
        {
            SetStatus($"Motion sensor error: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates status label and announces message for accessibility.
    /// </summary>
    private void SetStatus(string message)
    {
        HardwareStatusLabel.Text = message;
        // Screen reader announcement for blind users
        SemanticScreenReader.Announce(message);
    }
}
