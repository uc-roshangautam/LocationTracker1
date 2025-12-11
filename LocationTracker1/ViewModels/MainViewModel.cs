using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LocationTracker1.Models;
using LocationTracker1.Services;

namespace LocationTracker1.ViewModels
{
    /// <summary>
    /// ViewModel for the main location tracking interface.
    /// Implements MVVM pattern with INotifyPropertyChanged for data binding.
    /// Manages location tracking state, database operations, and UI updates.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly LocationService _locationService;
        private readonly DatabaseService _databaseService;
        private bool _isTracking;
        private string _statusMessage;
        private CancellationTokenSource? _trackingCts;

        /// <summary>
        /// Gets the observable collection of tracked locations.
        /// Automatically updates the UI when items are added or removed.
        /// </summary>
        public ObservableCollection<LocationData> Locations { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether location tracking is currently active.
        /// </summary>
        public bool IsTracking
        {
            get => _isTracking;
            set { _isTracking = value; OnPropertyChanged(); OnPropertyChanged(nameof(TrackingButtonText)); }
        }

        /// <summary>
        /// Gets or sets the status message displayed to the user.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets the text to display on the tracking toggle button.
        /// Returns "Stop Tracking" when active, "Start Tracking" when inactive.
        /// </summary>
        public string TrackingButtonText => IsTracking ? "Stop Tracking" : "Start Tracking";

        /// <summary>
        /// Gets the command to start or stop location tracking.
        /// </summary>
        public ICommand ToggleTrackingCommand { get; }

        /// <summary>
        /// Gets the command to clear all stored locations from the database.
        /// </summary>
        public ICommand ClearLocationsCommand { get; }

        /// <summary>
        /// Gets the command to reload locations from the database.
        /// </summary>
        public ICommand LoadLocationsCommand { get; }

        /// <summary>
        /// Occurs when a property value changes (for data binding).
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Occurs when the locations collection is updated (for map refresh).
        /// </summary>
        public event Action? LocationsUpdated;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// Sets up services and commands for location tracking operations.
        /// </summary>
        public MainViewModel()
        {
            _locationService = new LocationService();
            _databaseService = new DatabaseService();
            _statusMessage = "Ready to track";

            // Initialize commands with async delegates
            ToggleTrackingCommand = new Command(async () => await ToggleTrackingAsync());
            ClearLocationsCommand = new Command(async () => await ClearLocationsAsync());
            LoadLocationsCommand = new Command(async () => await LoadLocationsAsync());
        }

        /// <summary>
        /// Loads all saved location entries from the database.
        /// Updates the Locations collection and triggers a map refresh.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadLocationsAsync()
        {
            var locations = await _databaseService.GetAllLocationsAsync();
            Locations.Clear();
            foreach (var loc in locations)
            {
                Locations.Add(loc);
            }
            StatusMessage = $"Loaded {Locations.Count} locations";
            LocationsUpdated?.Invoke();
        }

        /// <summary>
        /// Toggles location tracking on or off.
        /// Starts tracking if currently stopped, stops tracking if currently active.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ToggleTrackingAsync()
        {
            if (IsTracking)
            {
                StopTracking();
            }
            else
            {
                await StartTrackingAsync();
            }
        }

        /// <summary>
        /// Starts continuous location tracking.
        /// Requests permissions and begins the tracking loop if granted.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task StartTrackingAsync()
        {
            // Check and request location permissions
            var hasPermission = await _locationService.CheckAndRequestPermissionAsync();
            if (!hasPermission)
            {
                StatusMessage = "Location permission denied";
                return;
            }

            IsTracking = true;
            _trackingCts = new CancellationTokenSource();
            StatusMessage = "Tracking started...";

            // Start tracking loop without blocking (fire and forget)
            _ = TrackLocationLoopAsync(_trackingCts.Token);
        }

        /// <summary>
        /// Stops the continuous location tracking.
        /// Cancels the tracking loop and updates the UI.
        /// </summary>
        private void StopTracking()
        {
            _trackingCts?.Cancel();
            IsTracking = false;
            StatusMessage = $"Tracking stopped. {Locations.Count} locations saved.";
        }

        /// <summary>
        /// Continuously tracks location at regular intervals (every 5 seconds).
        /// Saves each location to the database and updates the UI.
        /// </summary>
        /// <param name="token">Cancellation token to stop the tracking loop.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task TrackLocationLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Get current device location
                    var location = await _locationService.GetCurrentLocationAsync();
                    if (location != null)
                    {
                        // Create location data entry
                        var locationData = new LocationData
                        {
                            Latitude = location.Latitude,
                            Longitude = location.Longitude,
                            Accuracy = location.Accuracy
                        };

                        // Save to database and update collection
                        await _databaseService.SaveLocationAsync(locationData);
                        Locations.Add(locationData);
                        StatusMessage = $"Captured: {location.Latitude:F4}, {location.Longitude:F4}";
                        LocationsUpdated?.Invoke();
                    }

                    // Wait 5 seconds before next capture
                    await Task.Delay(5000, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Clears all saved locations from the database and UI.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ClearLocationsAsync()
        {
            await _databaseService.ClearAllLocationsAsync();
            Locations.Clear();
            StatusMessage = "All locations cleared";
            LocationsUpdated?.Invoke();
        }

        /// <summary>
        /// Raises the PropertyChanged event to notify the UI of property value changes.
        /// </summary>
        /// <param name="name">Name of the property that changed (automatically set by caller).</param>
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
