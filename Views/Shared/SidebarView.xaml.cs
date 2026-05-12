using CruzNeryClinic.ViewModels.Shared;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CruzNeryClinic.Views.Shared
{
    // SidebarView is a reusable sidebar UserControl.
    // It exposes navigation and logout events so MainShellView can respond.
    public partial class SidebarView : UserControl
    {
        private readonly SidebarViewModel _viewModel;

        public event Action<string>? NavigationRequested;
        public event Action? LogoutRequested;

        public SidebarView()
        {
            InitializeComponent();

            _viewModel = new SidebarViewModel();
            
            SidebarRoot.DataContext = _viewModel;

            _viewModel.NavigationRequested += moduleName =>
            {
                NavigationRequested?.Invoke(moduleName);
            };

            _viewModel.LogoutRequested += () =>
            {
                LogoutRequested?.Invoke();
            };
        }

        public static readonly DependencyProperty SelectedModuleProperty =
            DependencyProperty.Register(
                nameof(SelectedModule),
                typeof(string),
                typeof(SidebarView),
                new PropertyMetadata("Dashboard", OnSelectedModuleChanged)
            );

        public string SelectedModule
        {
            get => (string)GetValue(SelectedModuleProperty);
            set => SetValue(SelectedModuleProperty, value);
        }

        private static void OnSelectedModuleChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is SidebarView sidebarView &&
                e.NewValue is string selectedModule)
            {
                sidebarView._viewModel.SelectedModule = selectedModule;
            }
        }
    }
}