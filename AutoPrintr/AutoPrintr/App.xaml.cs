using AutoPrintr.Helpers;
using AutoPrintr.IServices;
using AutoPrintr.Services;
using AutoPrintr.ViewModels;
using AutoPrintr.Views;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
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

            Messenger.Default.Register<ShowControlMessage>(this, OnShowControl);
            Messenger.Default.Register<HideControlMessage>(this, OnHideControl);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }
        #endregion

        #region DataContext and Navigation
        public static BaseViewModel GetDataContext(ViewType view)
        {
            switch (view)
            {
                case ViewType.ContextMenu: return SimpleIoc.Default.GetInstance<TrayIconContextMenuViewModel>();
                case ViewType.Login: return SimpleIoc.Default.GetInstance<LoginViewModel>();
                case ViewType.Settings: return SimpleIoc.Default.GetInstance<SettingsViewModel>();
                case ViewType.Jobs: return SimpleIoc.Default.GetInstance<JobsViewModel>();
                case ViewType.Logs: return SimpleIoc.Default.GetInstance<LogsViewModel>();
                case ViewType.About: return SimpleIoc.Default.GetInstance<AboutViewModel>();
                default: return null;
            }
        }

        public void NavigatedTo(ViewType view, object parm)
        {
            var dataContext = GetDataContext(view);
            dataContext.NavigatedTo(parm);
        }
        #endregion

        #region Startup and Exit and Types
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            RegisterTypes();
            await LoadSettingsAsync();
            await RunJobsAsync();

            //Display Tray Icon
            new TrayIconContextMenuView();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            //Close Tray Icon
            TrayIconContextMenuView.Close();
        }

        protected override async void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            await StopJobsAsync();

            base.OnSessionEnding(e);
        }

        private void RegisterTypes()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            var emailSettings = new EmailSettings
            {
                SupportEmailAddress = (string)Resources[nameof(EmailSettings.SupportEmailAddress)],
                SupportEmailSubject = (string)Resources[nameof(EmailSettings.SupportEmailSubject)]
            };
            SimpleIoc.Default.Register(() => emailSettings);

            //Register Services
            SimpleIoc.Default.Register<ILoggerService, LoggerService>();
            SimpleIoc.Default.Register<IApiService, ApiService>();
            SimpleIoc.Default.Register<IFileService>(() => new FileService());
            SimpleIoc.Default.Register<ISettingsService, SettingsService>();
            SimpleIoc.Default.Register<IUserService, UserService>();
            SimpleIoc.Default.Register<IPrinterService, PrinterService>();
            SimpleIoc.Default.Register<IJobsService, JobsService>();
            SimpleIoc.Default.Register<INavigationService, NavigationService>();

            //Register ViewModels
            SimpleIoc.Default.Register<TrayIconContextMenuViewModel>(true);
            SimpleIoc.Default.Register<LoginViewModel>(true);
            SimpleIoc.Default.Register<SettingsViewModel>(true);
            SimpleIoc.Default.Register<JobsViewModel>(true);
            SimpleIoc.Default.Register<LogsViewModel>(true);
            SimpleIoc.Default.Register<AboutViewModel>(true);
        }

        private async Task LoadSettingsAsync()
        {
            var settingsService = SimpleIoc.Default.GetInstance<ISettingsService>();
            await settingsService.LoadSettingsAsync();

            Messenger.Default.Send(settingsService.Settings.User);
        }

        private async Task RunJobsAsync()
        {
            var jobsService = SimpleIoc.Default.GetInstance<IJobsService>();
            await jobsService.RunAsync();
        }

        private async Task StopJobsAsync()
        {
            var jobsService = SimpleIoc.Default.GetInstance<IJobsService>();
            await jobsService.StopAsync();
        }
        #endregion

        #region Messages
        private void OnShowControl(ShowControlMessage message)
        {
            switch (message.Type)
            {
                //case ControlMessageType.Busy: BusyControl.Show(message.Caption); break;
                case ControlMessageType.Message: MessageBox.Show((string)message.Data, message.Caption); break;
                default: break;
            }
        }

        private void OnHideControl(HideControlMessage message)
        {
            switch (message.Type)
            {
                //case ControlMessageType.Busy: BusyControl.Hide(); break;
                default: break;
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