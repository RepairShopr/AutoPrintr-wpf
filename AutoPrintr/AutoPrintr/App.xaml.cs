using AutoPrintr.Core.IServices;
using AutoPrintr.Helpers;
using AutoPrintr.Views;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Diagnostics;
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
        }
        #endregion

        #region Startup and Exit
        protected override async void OnStartup(StartupEventArgs e)
        {
            var processName = Process.GetCurrentProcess().ProcessName;
            if (Process.GetProcessesByName(processName).LongLength > 1)
                Process.GetCurrentProcess().Kill();

            base.OnStartup(e);
            await WpfApp.Instance.Startup();
            new TrayIconContextMenuView();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            TrayIconContextMenuView.Close();
            base.OnExit(e);
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