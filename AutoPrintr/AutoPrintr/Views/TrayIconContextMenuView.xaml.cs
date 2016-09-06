using System;
using System.Reflection;
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

            string appName = Assembly.GetEntryAssembly().GetName().Name;
            _notifier = new System.Windows.Forms.NotifyIcon()
            {
                Text = appName,
                Icon = AutoPrintr.Resources.Printer_32,
                Visible = true
            };
            _notifier.Click += _notifier_Click;

            _notifier.ShowBalloonTip(500, appName, $"The {appName} service has been started", System.Windows.Forms.ToolTipIcon.Info);
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