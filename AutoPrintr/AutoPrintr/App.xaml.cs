using AutoPrintr.Controls;
using System.Windows;

namespace AutoPrintr
{
    internal partial class App : Application
    {
        public App()
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            new NotifyIconContextMenu();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            NotifyIconContextMenu.Close();
        }
    }
}