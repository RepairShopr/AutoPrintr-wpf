using System;
using System.Windows;
using System.Windows.Controls;

namespace AutoPrintr.Views
{
    internal partial class TrayIconContextMenuView : UserControl
    {
        private static System.Windows.Forms.NotifyIcon _notifier;

        public TrayIconContextMenuView()
        {
            InitializeComponent();
            CreateTrayIcon();

            ContextMenu.DataContext = App.GetDataContext(Helpers.ViewType.ContextMenu);
        }

        public static void Close()
        {
            _notifier?.Dispose();
            _notifier = null;
        }

        private void CreateTrayIcon()
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

            _notifier.ShowBalloonTip(500);
        }

        private void _notifier_Click(object sender, EventArgs e)
        {
            ContextMenu.IsOpen = true;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}