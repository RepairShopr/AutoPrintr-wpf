using AutoPrintr.Services;
using AutoPrintr.ViewModels;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;

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