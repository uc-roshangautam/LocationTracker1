using SQLite;
using LocationTracker1.Models;

namespace LocationTracker1.Services
{
    /// <summary>
    /// Provides data access operations for location tracking using SQLite database.
    /// Implements async methods for better performance and UI responsiveness.
    /// </summary>
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;
        private readonly string _dbPath;

        /// <summary>
        /// Initializes a new instance of the DatabaseService class.
        /// Sets up the database file path in the application's data directory.
        /// </summary>
        public DatabaseService()
        {
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "locations.db3");
        }

        /// <summary>
        /// Initializes the database connection and creates the LocationData table if it doesn't exist.
        /// This method is called automatically before any database operation.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task InitAsync()
        {
            if (_database != null)
                return;

            _database = new SQLiteAsyncConnection(_dbPath);
            await _database.CreateTableAsync<LocationData>();
        }

        /// <summary>
        /// Saves a location data entry to the database.
        /// </summary>
        /// <param name="location">The LocationData object to save.</param>
        /// <returns>The number of rows inserted (typically 1).</returns>
        public async Task<int> SaveLocationAsync(LocationData location)
        {
            await InitAsync();
            return await _database!.InsertAsync(location);
        }

        /// <summary>
        /// Retrieves all location entries from the database.
        /// </summary>
        /// <returns>A list of all LocationData entries stored in the database.</returns>
        public async Task<List<LocationData>> GetAllLocationsAsync()
        {
            await InitAsync();
            return await _database!.Table<LocationData>().ToListAsync();
        }

        /// <summary>
        /// Deletes all location entries from the database.
        /// </summary>
        /// <returns>The number of rows deleted.</returns>
        public async Task<int> ClearAllLocationsAsync()
        {
            await InitAsync();
            return await _database!.DeleteAllAsync<LocationData>();
        }
    }
}
