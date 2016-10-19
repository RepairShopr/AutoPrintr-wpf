using AutoPrintr.Core.IServices;
using AutoPrintr.Services;
using AutoPrintr.ViewModels;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AutoPrintr.Helpers
{
    internal class WpfApp : Core.App
    {
        #region Properties
        private static readonly WpfApp _instance = new WpfApp();

        public static WpfApp Instance => _instance;
        #endregion

        #region Constructors
        static WpfApp()
        { }

        private WpfApp()
        {
            Messenger.Default.Register<ShowControlMessage>(this, OnShowControl);
            Messenger.Default.Register<HideControlMessage>(this, OnHideControl);
        }
        #endregion

        #region Methods
        protected override void RegisterTypes()
        {
            base.RegisterTypes();

            SimpleIoc.Default.Register<EmailSettings>();
            SimpleIoc.Default.Register<INavigationService, NavigationService>();

            //Register ViewModels
            SimpleIoc.Default.Register<TrayIconContextMenuViewModel>(true);
            SimpleIoc.Default.Register<LoginViewModel>(true);
            SimpleIoc.Default.Register<SettingsViewModel>(true);
            SimpleIoc.Default.Register<JobsViewModel>(true);
            SimpleIoc.Default.Register<LogsViewModel>(true);
            SimpleIoc.Default.Register<AboutViewModel>(true);
        }

        protected override async Task<bool> LoadSettingsAsync()
        {
            var settingsService = SimpleIoc.Default.GetInstance<ISettingsService>();

            var result = await base.LoadSettingsAsync();
            if (!result)
            {
                settingsService.AddToStartup(true);
                InstallService();
            }

            Messenger.Default.Send(settingsService.Settings.User);

            return result;
        }

        protected override Task InitializeLogsAsync()
        {
            var loggerService = SimpleIoc.Default.GetInstance<ILoggerService>();
            return loggerService.InitializeAppLogsAsync();
        }

        #region DataContext and Navigation
        public BaseViewModel GetDataContext(ViewType view)
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

        #region Messages
        private void OnShowControl(ShowControlMessage message)
        {
            switch (message.Type)
            {
                //case ControlMessageType.Busy: BusyControl.Show(message.Caption); break;
                case ControlMessageType.Message: System.Windows.MessageBox.Show((string)message.Data, message.Caption); break;
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

        #region Service
        private void InstallService()
        {
            var loggingService = SimpleIoc.Default.GetInstance<ILoggerService>();
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = typeof(Service.Installer).Assembly.Location,
                    Arguments = $"/{Service.Helpers.Commands.Stop} /{Service.Helpers.Commands.Uninstall} /{Service.Helpers.Commands.Install} /{Service.Helpers.Commands.Start}",
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };

                var process = Process.Start(psi);
                process.WaitForExit();

                loggingService.WriteInformation($"Service is installed");
            }
            catch (Exception ex)
            {
                loggingService.WriteWarning($"Service is not installed");

                Debug.WriteLine($"Error in {nameof(WpfApp)}: {ex.ToString()}");
                loggingService.WriteError(ex);
            }
        }
        #endregion

        #endregion
    }
}