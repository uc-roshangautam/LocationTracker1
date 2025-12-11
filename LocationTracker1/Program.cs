using UIKit;

namespace LocationTracker1
{
    /// <summary>
    /// Entry point for the macOS/iOS application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }

    /// <summary>
    /// AppDelegate for macOS/iOS MAUI application.
    /// </summary>
    public class AppDelegate : MauiUIApplicationDelegate
    {
        /// <summary>
        /// Creates the MAUI app instance.
        /// </summary>
        /// <returns>The configured MAUI app.</returns>
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}

