using AutoPrintr.Core.IServices;
using AutoPrintr.Services;
using AutoPrintr.ViewModels;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using System.Deployment.Application;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace AutoPrintr.Helpers
{
    internal class WpfApp : Core.App
    {
        #region Properties
        private static readonly WpfApp _instance = new WpfApp();

        public static WpfApp Instance => _instance;

        private ILoggerService LoggerService => SimpleIoc.Default.GetInstance<ILoggerService>();
        private ISettingsService SettingsService => SimpleIoc.Default.GetInstance<ISettingsService>();
        private INavigationService NavigationService => SimpleIoc.Default.GetInstance<INavigationService>();
        private IWindowsServiceClient WindowsServiceClient => SimpleIoc.Default.GetInstance<IWindowsServiceClient>();
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
        public override async Task Startup(string[] args)
        {
            var processName = Process.GetCurrentProcess().ProcessName;
            if (Process.GetProcessesByName(processName).LongLength > 1)
            {
                Process.GetCurrentProcess().Kill();
                return;
            }

            await base.Startup(args);

            if (SettingsService.Settings.User == null)
                NavigationService.NavigateTo(ViewType.Login.ToString());

            ConnectWindowsServiceClient();

            //CheckForUpdates();
        }

        public async Task Stop()
        {
            await WindowsServiceClient.DisconnectAsync();
        }

        protected override void RegisterTypes()
        {
            base.RegisterTypes();

            SimpleIoc.Default.Register<EmailSettings>();
            SimpleIoc.Default.Register<INavigationService, NavigationService>();
            SimpleIoc.Default.Register<IWindowsServiceClient, WindowsServiceClient>();

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
            var result = await base.LoadSettingsAsync();

            SettingsService.MonitorSettingsChanges();

            if (!result)
                await SettingsService.AddToStartup(true);

            if (!SettingsService.Settings.InstalledService)
                await SettingsService.InstallService(true);

            Messenger.Default.Send(SettingsService.Settings.User);

            return result;
        }

        protected override Task InitializeLogsAsync()
        {
            return LoggerService.InitializeAppLogsAsync();
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
                case ControlMessageType.Message: MessageBox.Show((string)message.Data, message.Caption, MessageBoxButton.OK, MessageBoxImage.Information); break;
                case ControlMessageType.Warning: MessageBox.Show((string)message.Data, message.Caption, MessageBoxButton.OK, MessageBoxImage.Warning); break;
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

        #region Updates
        private async void CheckForUpdates()
        {
            UpdateCheckInfo info = null;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                var deployment = ApplicationDeployment.CurrentDeployment;

                try
                {
                    info = deployment.CheckForDetailedUpdate();
                }
                catch (DeploymentDownloadException dde)
                {
                    Debug.WriteLine($"Error in {nameof(App)}: {dde.ToString()}");

                    LoggerService.WriteWarning("The new version of the application cannot be downloaded at this time. Please check your network connection, or try again later");
                    LoggerService.WriteError(dde);

                    return;
                }
                catch (InvalidDeploymentException ide)
                {
                    Debug.WriteLine($"Error in {nameof(App)}: {ide.ToString()}");

                    LoggerService.WriteWarning("Cannot check for a new version of the application. The ClickOnce deployment is corrupt. Please redeploy the application and try again");
                    LoggerService.WriteError(ide);

                    return;
                }

                if (info.UpdateAvailable)
                {
                    var caption = "Updates are Available";
                    var message = "A newer version of AutoPrintr is available. Would you like to install it now?";
                    if (MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
                        return;

                    try
                    {
                        if (SettingsService.Settings.InstalledService)
                            await SettingsService.InstallService(false);

                        if (SettingsService.Settings.InstalledService)
                        {
                            MessageBox.Show("AutoPrintr Service can not be stopped and uninstalled. Please uninstall service and try again", "Updates are not Installed", MessageBoxButton.OK, MessageBoxImage.Warning);

                            LoggerService.WriteWarning("Cannot install the latest version of the application. Service can not be stopped and uninstalled");

                            return;
                        }

                        deployment.Update();

                        Process.Start(deployment.UpdatedApplicationFullName);
                        Application.Current.Shutdown();
                    }
                    catch (DeploymentDownloadException dde)
                    {
                        MessageBox.Show("Cannot install the latest version of the application. Please check your network connection, or try again later", "Updates are not Installed", MessageBoxButton.OK, MessageBoxImage.Error);

                        Debug.WriteLine($"Error in {nameof(App)}: {dde.ToString()}");

                        LoggerService.WriteWarning("Cannot install the latest version of the application. Please check your network connection, or try again later");
                        LoggerService.WriteError(dde);
                    }
                }
            }
        }
        #endregion

        #region Windows Service Client
        private async void ConnectWindowsServiceClient()
        {
            var settingsViewModel = SimpleIoc.Default.GetInstance<SettingsViewModel>();
            await WindowsServiceClient.ConnectAsync(settingsViewModel.ShowConnectionFailedMessage);
        }
        #endregion

        #endregion
    }
}