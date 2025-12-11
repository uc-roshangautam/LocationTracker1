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
                // Request location with best accuracy and 10-second timeout
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request);
                return location;
            }
            catch (FeatureNotSupportedException)
            {
                // Location services not supported on this device
                return null;
            }
            catch (FeatureNotEnabledException)
            {
                // Location services are disabled on the device
                return null;
            }
            catch (PermissionException)
            {
                // Location permission not granted
                return null;
            }
            catch (Exception)
            {
                // Other errors (e.g., timeout, connection issues)
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
