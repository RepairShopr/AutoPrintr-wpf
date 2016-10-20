using AutoPrintr.Core.IServices;
using AutoPrintr.Services;
using AutoPrintr.ViewModels;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
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
                var caption = "Application Startup";
                var message = "It's a first run. Would you like to add an App to the windows startup?";
                if (System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                    await settingsService.AddToStartup(true);

                caption = "Install Service";
                message = "Would you like to install the service?";
                if (System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                    await settingsService.InstallService(true);
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

        #endregion
    }
}