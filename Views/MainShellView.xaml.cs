using CruzNeryClinic.ViewModels;
using System.Windows.Controls;

namespace CruzNeryClinic.Views
{
    // MainShellView is the logged-in shell of the app.
    // It contains the reusable sidebar and a content area for modules.
    public partial class MainShellView : UserControl
    {
        public MainShellView()
        {
            InitializeComponent();

            // Forward sidebar navigation to the shell ViewModel.
            Sidebar.NavigationRequested += moduleName =>
            {
                if (DataContext is MainShellViewModel viewModel)
                {
                    viewModel.NavigateTo(moduleName);
                }
            };

            // Forward sidebar logout to the shell ViewModel.
            Sidebar.LogoutRequested += () =>
            {
                if (DataContext is MainShellViewModel viewModel)
                {
                    viewModel.Logout();
                }
            };
        }
    }
}