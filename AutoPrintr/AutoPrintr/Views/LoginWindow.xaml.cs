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