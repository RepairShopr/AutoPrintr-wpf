using System;
using System.Windows.Controls;

namespace AutoPrintr.Controls
{
    internal partial class NotifyIconContextMenu : UserControl
    {
        private static System.Windows.Forms.NotifyIcon _notifier;

        public NotifyIconContextMenu()
        {
            InitializeComponent();
            CreateNotifier();
        }

        public static void Close()
        {
            _notifier?.Dispose();
            _notifier = null;
        }

        private void CreateNotifier()
        {
            if (_notifier != null)
                throw new InvalidOperationException();

            _notifier = new System.Windows.Forms.NotifyIcon()
            {
                BalloonTipText = "The AutoPrintr service has been started",
                BalloonTipTitle = "AutoPrintr",
                Text = "AutoPrintr",
                Icon = AutoPrintr.Resources.Printer_32,
                Visible = true
            };
            _notifier.Click += _notifier_Click;

            _notifier.ShowBalloonTip(200);
        }

        private void _notifier_Click(object sender, EventArgs e)
        {
            ContextMenu.IsOpen = true;
        }

        private void ExitMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void LoginMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void JobsMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void SettingsMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void LogoutMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}