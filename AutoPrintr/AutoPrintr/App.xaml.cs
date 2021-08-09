using AutoPrintr.Core.IServices;
using AutoPrintr.Helpers;
using AutoPrintr.Views;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AutoPrintr
{
    internal partial class App : Application
    {
        #region Constructors
        public App()
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            ServicePointManager.SecurityProtocol
                = SecurityProtocolType.Tls
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls12
                ;
        }
        #endregion

        #region Startup and Exit
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!CheckIsNet45Installed())
            {
                var message = "This application require .NET Framework 4.5 or above. Would you like to install the latest version of .NET Framework now?";
                var caption = ".NET Framework 4.5 is not installed";

                if (MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    var downloadUrl = "https://www.microsoft.com/en-US/download/details.aspx?id=49981";
                    Process.Start(downloadUrl);
                }

                Process.GetCurrentProcess().Kill();
                return;
            }

            await WpfApp.Instance.Startup(e.Args);
            var trayIcon = new TrayIconContextMenuView();
            if (WpfApp.Instance.FirstStart)
            {
                trayIcon.ShowBalloonTip();
            }

            RefreshTrayArea();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await WpfApp.Instance.Stop();
            TrayIconContextMenuView.Close();
            base.OnExit(e);
        }

        private bool CheckIsNet45Installed()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                var netVersion = (int?)ndpKey?.GetValue("Release");
                //Version of .NET Framework 4.5 is 378389
                return netVersion >= 378389;
            }
        }

        private static void RefreshTrayArea()
        {
            ILoggerService logger = null;
            try
            {
                logger = SimpleIoc.Default.GetInstance<ILoggerService>();
                Task.Run(() => SystemTrayUpdater.Refresh(logger));
            }
            catch (Exception e)
            {
                logger?.WriteError($"Error refresh tray area. {e}");
            }
        }
        #endregion

        #region Exceptions
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Error in {nameof(App)}: {((Exception)e.ExceptionObject).ToString()}");

            var loggingService = SimpleIoc.Default.GetInstance<ILoggerService>();
            loggingService.WriteError((Exception)e.ExceptionObject);
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Error in {nameof(App)}: {e.Exception.ToString()}");

            var loggingService = SimpleIoc.Default.GetInstance<ILoggerService>();
            loggingService.WriteError(e.Exception);
            e.Handled = true;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Error in {nameof(App)}: {e.Exception.ToString()}");

            var loggingService = SimpleIoc.Default.GetInstance<ILoggerService>();
            loggingService.WriteError(e.Exception);
            e.Handled = true;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Debug.WriteLine($"Error in {nameof(App)}: {e.Exception.ToString()}");

            var loggingService = SimpleIoc.Default.GetInstance<ILoggerService>();
            loggingService.WriteError(e.Exception);
            e.SetObserved();
        }
        #endregion
    }
}