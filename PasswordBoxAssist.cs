using System.Windows;
using System.Windows.Controls;

namespace CruzNeryClinic
{
    public static class PasswordBoxAssist
    {
        public static readonly DependencyProperty MonitorPasswordProperty =
            DependencyProperty.RegisterAttached(
                "MonitorPassword",
                typeof(bool),
                typeof(PasswordBoxAssist),
                new PropertyMetadata(false, OnMonitorPasswordChanged));

        public static readonly DependencyProperty IsPasswordEmptyProperty =
            DependencyProperty.RegisterAttached(
                "IsPasswordEmpty",
                typeof(bool),
                typeof(PasswordBoxAssist),
                new PropertyMetadata(true));

        public static void SetMonitorPassword(DependencyObject element, bool value)
            => element.SetValue(MonitorPasswordProperty, value);

        public static bool GetMonitorPassword(DependencyObject element)
            => (bool)element.GetValue(MonitorPasswordProperty);

        public static void SetIsPasswordEmpty(DependencyObject element, bool value)
            => element.SetValue(IsPasswordEmptyProperty, value);

        public static bool GetIsPasswordEmpty(DependencyObject element)
            => (bool)element.GetValue(IsPasswordEmptyProperty);

        private static void OnMonitorPasswordChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            if (element is not PasswordBox passwordBox)
                return;

            passwordBox.PasswordChanged -= PasswordBoxPasswordChanged;
            passwordBox.Loaded -= PasswordBoxLoaded;

            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged += PasswordBoxPasswordChanged;
                passwordBox.Loaded += PasswordBoxLoaded;
                UpdateIsPasswordEmpty(passwordBox);
            }
        }

        private static void PasswordBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
                UpdateIsPasswordEmpty(passwordBox);
        }

        private static void PasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
                UpdateIsPasswordEmpty(passwordBox);
        }

        private static void UpdateIsPasswordEmpty(PasswordBox passwordBox)
            => SetIsPasswordEmpty(passwordBox, string.IsNullOrEmpty(passwordBox.Password));
    }
}
