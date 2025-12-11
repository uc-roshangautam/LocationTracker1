namespace LocationTracker1.Services
{
    /// <summary>
    /// Provides location tracking functionality using the device's GPS/location services.
    /// Handles permission requests and location retrieval with proper error handling.
    /// </summary>
    public class LocationService
    {
        /// <summary>
        /// Retrieves the current geographical location of the device.
        /// Uses best accuracy setting with a 10-second timeout.
        /// </summary>
        /// <returns>
        /// The current Location if successful; null if location services are unavailable,
        /// disabled, or permissions are denied.
        /// </returns>
        public async Task<Location?> GetCurrentLocationAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[LocationService] Starting location request...");
                
                // Try to get last known location first (faster)
                var lastLocation = await Geolocation.Default.GetLastKnownLocationAsync();
                if (lastLocation != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[LocationService] Got last known location: {lastLocation.Latitude:F4}, {lastLocation.Longitude:F4}");
                    return lastLocation;
                }
                
                System.Diagnostics.Debug.WriteLine("[LocationService] No last known location, requesting current...");
                
                // Request location with best accuracy and 10-second timeout
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request);
                
                if (location != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[LocationService] Got current location: {location.Latitude:F4}, {location.Longitude:F4}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LocationService] Current location returned null");
                }
                
                return location;
            }
            catch (FeatureNotSupportedException ex)
            {
                // Location services not supported on this device
                System.Diagnostics.Debug.WriteLine($"[LocationService] FeatureNotSupported: {ex.Message}");
                return null;
            }
            catch (FeatureNotEnabledException ex)
            {
                // Location services are disabled on the device
                System.Diagnostics.Debug.WriteLine($"[LocationService] FeatureNotEnabled: {ex.Message}");
                return null;
            }
            catch (PermissionException ex)
            {
                // Location permission not granted
                System.Diagnostics.Debug.WriteLine($"[LocationService] PermissionException: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                // Other errors (e.g., timeout, connection issues)
                System.Diagnostics.Debug.WriteLine($"[LocationService] Exception: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks if location permission is granted and requests it if necessary.
        /// </summary>
        /// <returns>
        /// True if location permission is granted; false otherwise.
        /// </returns>
        public async Task<bool> CheckAndRequestPermissionAsync()
        {
            // Check current permission status
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            // Request permission if not already granted
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            return status == PermissionStatus.Granted;
        }
    }
}
