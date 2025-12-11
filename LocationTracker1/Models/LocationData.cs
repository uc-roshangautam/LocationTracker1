using SQLite;

namespace LocationTracker1.Models
{
    /// <summary>
    /// Represents a geographical location data point with timestamp.
    /// This entity is stored in the SQLite database to track user location history.
    /// </summary>
    public class LocationData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the location entry.
        /// This is auto-incremented by the database.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the latitude coordinate in decimal degrees.
        /// Valid range: -90.0 to +90.0
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude coordinate in decimal degrees.
        /// Valid range: -180.0 to +180.0
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp when this location was recorded.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the accuracy of the location reading in meters.
        /// Lower values indicate more accurate readings. Nullable if accuracy is unknown.
        /// </summary>
        public double? Accuracy { get; set; }

        /// <summary>
        /// Initializes a new instance of the LocationData class.
        /// Sets the timestamp to the current UTC time.
        /// </summary>
        public LocationData()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
