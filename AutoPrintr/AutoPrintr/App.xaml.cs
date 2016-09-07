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
using System.Threading.Tasks;
using System.Windows;

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
                default: return null;
            }
        }

        public void NavigatedTo(ViewType view, object parm)
        {
            var dataContext = GetDataContext(view);
            dataContext.NavigatedTo(parm);
        }
        #endregion

        #region Startup and Exit
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

            //Register Services
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
            throw new NotImplementedException();
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}