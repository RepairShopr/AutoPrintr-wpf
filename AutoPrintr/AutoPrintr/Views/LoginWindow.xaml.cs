using AutoPrintr.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AutoPrintr.Views
{
    internal partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += LoginWindow_Loaded;
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= LoginWindow_Loaded;
#if DEBUG
            UsernameTextBox.Text = "geruch.vitaliy@gmail.com";
            PasswordBox.Password = "test@123";
#endif
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            ((LoginViewModel)DataContext).Login.Password = passwordBox.Password;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}