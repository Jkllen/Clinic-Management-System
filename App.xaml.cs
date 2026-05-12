using CruzNeryClinic.Data;
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace CruzNeryClinic
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Catches normal UI thread errors.
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Catches non-UI thread errors.
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                // Initialize the database before showing MainWindow.
                DatabaseInitializer.Initialize();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                ShowAndSaveError(ex, "Startup Error");
                Shutdown();
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowAndSaveError(e.Exception, "Dispatcher UI Error");
            e.Handled = true;
            Shutdown();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                ShowAndSaveError(ex, "Unhandled Application Error");
        }

        private void ShowAndSaveError(Exception ex, string title)
        {
            string appFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CruzNeryClinic"
            );

            Directory.CreateDirectory(appFolder);

            string logPath = Path.Combine(appFolder, "startup-error-log.txt");

            File.WriteAllText(logPath, ex.ToString());

            MessageBox.Show(
                ex.ToString() + $"\n\nError log saved to:\n{logPath}",
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        } 
    }
}