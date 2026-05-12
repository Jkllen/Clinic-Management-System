using System;
using System.Threading.Tasks;

namespace CruzNeryClinic.ViewModels
{
    // SplashViewModel controls the behavior of the splash screen.
    // It waits for a short time, then tells the app to move to the login screen.
    public class SplashViewModel : BaseViewModel
    {
        // This event is triggered after the splash delay finishes.
        // MainWindow will listen to this event and then show LoginView.
        public event Action? SplashFinished;

        // Starts the splash screen loading process.
        public async Task StartSplashAsync()
        {
            // Small delay so the splash screen is visible to the user.
            // Adjust this if want it to be faster or slower.
            await Task.Delay(2000);

            // Notify the main window that splash is finished.
            SplashFinished?.Invoke();
        }
    }
}