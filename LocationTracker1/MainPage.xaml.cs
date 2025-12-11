using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using LocationTracker1.ViewModels;

namespace LocationTracker1
{
    /// <summary>
    /// Main page of the location tracking application.
    /// Displays a map with location pins and heat map visualization,
    /// along with controls for tracking, clearing, and refreshing data.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the MainPage class.
        /// Sets up the view model, event handlers, and loads initial location data.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            _viewModel.LocationsUpdated += UpdateMapPins;
            
            // Load saved locations on startup
            Dispatcher.Dispatch(async () =>
            {
                await _viewModel.LoadLocationsAsync();
                UpdateMapPins();
            });
        }

        /// <summary>
        /// Handles the click event for the tracking toggle button.
        /// Starts or stops location tracking based on current state.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnTrackingClicked(object sender, EventArgs e)
        {
            // If starting tracking, check permissions first
            if (!_viewModel.IsTracking)
            {
                StatusLabel.Text = "Checking location permissions...";
                
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                
                if (status != PermissionStatus.Granted)
                {
                    StatusLabel.Text = "Requesting location permission...";
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
                
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlertAsync("Permission Denied", 
                        "Location permission is required to track your location. Please enable it in System Settings > Privacy & Security > Location Services.", 
                        "OK");
                    StatusLabel.Text = "Location permission denied";
                    return;
                }
                
                StatusLabel.Text = "Permission granted! Starting tracking...";
            }
            
            _viewModel.ToggleTrackingCommand.Execute(null);
            TrackingButton.Text = _viewModel.TrackingButtonText;
            UpdateStatus();
        }

        /// <summary>
        /// Handles the click event for the clear button.
        /// Prompts user for confirmation before deleting all saved locations.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnClearClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlertAsync("Clear All", "Delete all saved locations?", "Yes", "No");
            if (confirm)
            {
                _viewModel.ClearLocationsCommand.Execute(null);
                UpdateStatus();
            }
        }

        /// <summary>
        /// Handles the click event for the refresh button.
        /// Reloads all locations from the database and updates the map.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnRefreshClicked(object sender, EventArgs e)
        {
            _viewModel.LoadLocationsCommand.Execute(null);
            UpdateStatus();
        }

        /// <summary>
        /// Updates the map with pins and heat map visualization for all saved locations.
        /// Creates red circles around each location point to simulate a heat map effect.
        /// Centers the map on the average position of all locations.
        /// </summary>
        private void UpdateMapPins()
        {
            // Clear existing pins and map elements
            LocationMap.Pins.Clear();
            LocationMap.MapElements.Clear();

            var locations = _viewModel.Locations.ToList();

            // If no locations, try to get current location and center on it
            if (locations.Count == 0)
            {
                // Try to get current location asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var location = await Geolocation.Default.GetLastKnownLocationAsync();
                        if (location == null)
                        {
                            location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
                            {
                                DesiredAccuracy = GeolocationAccuracy.Medium,
                                Timeout = TimeSpan.FromSeconds(5)
                            });
                        }
                        
                        if (location != null)
                        {
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                                    new Location(location.Latitude, location.Longitude),
                                    Distance.FromKilometers(5)));
                            });
                        }
                    }
                    catch
                    {
                        // If location fails, show a reasonable worldwide view
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                                new Location(0, 0),
                                Distance.FromKilometers(10000)));
                        });
                    }
                });
                return;
            }

            // Add pins for each location point
            foreach (var loc in locations)
            {
                var pin = new Pin
                {
                    Label = $"Point {loc.Id}",
                    Address = $"{loc.Timestamp:g}",
                    Location = new Location(loc.Latitude, loc.Longitude),
                    Type = PinType.Place
                };
                LocationMap.Pins.Add(pin);
            }

            // Add heat map circles for visualization
            // Use color gradient based on recency: newer = red/hot, older = blue/cool
            var oldestTime = locations.Min(l => l.Timestamp);
            var newestTime = locations.Max(l => l.Timestamp);
            var timeRange = (newestTime - oldestTime).TotalSeconds;

            foreach (var loc in locations)
            {
                // Calculate color based on timestamp (0 = oldest/blue, 1 = newest/red)
                var normalizedTime = timeRange > 0 
                    ? (loc.Timestamp - oldestTime).TotalSeconds / timeRange 
                    : 1.0;

                // Create gradient from blue (cold/old) to yellow to red (hot/new)
                Color strokeColor, fillColor;
                if (normalizedTime < 0.5)
                {
                    // Blue to Yellow transition (older data)
                    var ratio = normalizedTime * 2;
                    strokeColor = Color.FromRgba(
                        (int)(0 + ratio * 255),      // R: 0 → 255
                        (int)(100 + ratio * 155),    // G: 100 → 255
                        (int)(255 - ratio * 255),    // B: 255 → 0
                        0.6);                        // Alpha
                    fillColor = Color.FromRgba(
                        (int)(0 + ratio * 255),
                        (int)(100 + ratio * 155),
                        (int)(255 - ratio * 255),
                        0.3);
                }
                else
                {
                    // Yellow to Red transition (newer data)
                    var ratio = (normalizedTime - 0.5) * 2;
                    strokeColor = Color.FromRgba(
                        255,                         // R: stays at 255
                        (int)(255 - ratio * 255),    // G: 255 → 0
                        0,                           // B: stays at 0
                        0.6);                        // Alpha
                    fillColor = Color.FromRgba(
                        255,
                        (int)(255 - ratio * 255),
                        0,
                        0.3);
                }

                var circle = new Circle
                {
                    Center = new Location(loc.Latitude, loc.Longitude),
                    Radius = Distance.FromMeters(50),
                    StrokeColor = strokeColor,
                    StrokeWidth = 2,
                    FillColor = fillColor
                };
                LocationMap.MapElements.Add(circle);
            }

            // Center map on the average location
            var centerLat = locations.Average(l => l.Latitude);
            var centerLon = locations.Average(l => l.Longitude);
            LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(centerLat, centerLon),
                Distance.FromKilometers(1)));

            UpdateStatus();
        }

        /// <summary>
        /// Updates the status label with the current view model status message.
        /// </summary>
        private void UpdateStatus()
        {
            StatusLabel.Text = _viewModel.StatusMessage;
        }

        /// <summary>
        /// Handles the click event for the demo button.
        /// Adds simulated location data to demonstrate the heat map visualization.
        /// Creates a realistic walking path pattern.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnDemoClicked(object sender, EventArgs e)
        {
            await DisplayAlertAsync("Demo Mode", 
                "This will add a realistic walking path with 20 location points to demonstrate the heat map visualization with color gradients (blue→yellow→red).", 
                "OK");
            
            // Get current location or use default
            var currentLat = 27.7172;
            var currentLon = 85.3240;
            
            try
            {
                var location = await Geolocation.Default.GetLastKnownLocationAsync();
                if (location != null)
                {
                    currentLat = location.Latitude;
                    currentLon = location.Longitude;
                }
            }
            catch { }
            
            // Create a realistic walking path - like walking around a block/campus
            // This simulates: Start → Walk North → Turn East → Walk South → Turn West → Return
            var demoLocations = new[]
            {
                // Starting point
                (currentLat, currentLon),
                
                // Walk north along a street (5 points)
                (currentLat + 0.0005, currentLon + 0.0001),
                (currentLat + 0.001, currentLon + 0.0002),
                (currentLat + 0.0015, currentLon + 0.0003),
                (currentLat + 0.002, currentLon + 0.0003),
                
                // Turn right and walk east (4 points)
                (currentLat + 0.0021, currentLon + 0.0008),
                (currentLat + 0.0022, currentLon + 0.0013),
                (currentLat + 0.0022, currentLon + 0.0018),
                (currentLat + 0.0021, currentLon + 0.0023),
                
                // Turn right and walk south (5 points)
                (currentLat + 0.0016, currentLon + 0.0024),
                (currentLat + 0.0011, currentLon + 0.0025),
                (currentLat + 0.0006, currentLon + 0.0025),
                (currentLat + 0.0001, currentLon + 0.0024),
                (currentLat - 0.0003, currentLon + 0.0023),
                
                // Turn right and walk west back towards start (5 points)
                (currentLat - 0.0004, currentLon + 0.0018),
                (currentLat - 0.0004, currentLon + 0.0013),
                (currentLat - 0.0003, currentLon + 0.0008),
                (currentLat - 0.0002, currentLon + 0.0004),
                
                // Return to near starting point
                (currentLat - 0.0001, currentLon + 0.0001),
                (currentLat, currentLon)
            };
            
            // Timestamps: simulate a 40-minute walk (2 minutes between each point)
            var startTime = DateTime.UtcNow.AddMinutes(-40);
            
            for (int i = 0; i < demoLocations.Length; i++)
            {
                var locationData = new Models.LocationData
                {
                    Latitude = demoLocations[i].Item1,
                    Longitude = demoLocations[i].Item2,
                    Timestamp = startTime.AddMinutes(i * 2), // 2 minutes apart
                    Accuracy = 5.0 + (i % 4) // Varies between 5-8 meters
                };
                
                await _viewModel.AddDemoLocationAsync(locationData);
            }
            
            StatusLabel.Text = $"Added {demoLocations.Length} demo locations - realistic walking path!";
            UpdateMapPins();
        }
    }
}
