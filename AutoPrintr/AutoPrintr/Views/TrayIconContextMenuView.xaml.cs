using AutoPrintr.Helpers;
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

            if (ContextMenu != null) 
                ContextMenu.DataContext = WpfApp.Instance.GetDataContext(ViewType.ContextMenu);
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
            _notifier.MouseClick += OnMouseClick;

            _notifier.ShowBalloonTip(500, appName, $"The {appName} service has been started", System.Windows.Forms.ToolTipIcon.Info);
        }

        private void OnMouseClick(object sender, System.Windows.Forms.MouseEventArgs mouseEventArgs)
        {
            try
            {
                if (ContextMenu == null) 
                    return;

                if (mouseEventArgs.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    foreach (System.Windows.Controls.MenuItem item in ContextMenu.Items)
                    {
                        if (item.Name == "Settings")
                        {
                            item.Command.Execute(null);
                            break;
                        }
                    }
                }
                else
                {
                    ContextMenu.IsOpen = true;
                }
            }
            catch
            {
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}