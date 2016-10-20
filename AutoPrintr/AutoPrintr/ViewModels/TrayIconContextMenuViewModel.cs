using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using AutoPrintr.Helpers;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System.Diagnostics;

namespace AutoPrintr.ViewModels
{
    public class TrayIconContextMenuViewModel : BaseViewModel
    {
        #region Properties
        private readonly ISettingsService _settingsService;
        private readonly ILoggerService _loggerService;
        private readonly EmailSettings _emailSettings;

        public override ViewType Type => ViewType.ContextMenu;

        private bool _isUserLoggedIn;
        public bool IsUserLoggedIn
        {
            get { return _isUserLoggedIn; }
            private set { Set(ref _isUserLoggedIn, value); }
        }

        public RelayCommand GoToLoginCommand { get; private set; }
        public RelayCommand GoToSettingsCommand { get; private set; }
        public RelayCommand GoToJobsCommand { get; private set; }
        public RelayCommand GoToLogsCommand { get; private set; }
        public RelayCommand GoToAboutCommand { get; private set; }
        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand RequestHelpCommand { get; private set; }
        #endregion

        #region Constructors
        public TrayIconContextMenuViewModel(INavigationService navigationService,
            ISettingsService settingsService,
            ILoggerService loggerService,
            EmailSettings emailSettings)
            : base(navigationService)
        {
            _settingsService = settingsService;
            _loggerService = loggerService;
            _emailSettings = emailSettings;

            GoToLoginCommand = new RelayCommand(OnGoToLogin);
            GoToSettingsCommand = new RelayCommand(OnGoToSettings);
            GoToJobsCommand = new RelayCommand(OnGoToJobs);
            GoToLogsCommand = new RelayCommand(OnGoToLogs);
            GoToAboutCommand = new RelayCommand(OnGoToAbout);
            LogoutCommand = new RelayCommand(OnLogout);
            RequestHelpCommand = new RelayCommand(OnRequestHelp);

            MessengerInstance.Register<User>(this, OnUserChanged);
        }
        #endregion

        #region Methods
        private void OnUserChanged(User obj)
        {
            IsUserLoggedIn = obj != null;
        }

        private void OnGoToLogin()
        {
            NavigateTo(ViewType.Login);
        }

        private void OnGoToJobs()
        {
            NavigateTo(ViewType.Jobs);
        }

        private void OnGoToSettings()
        {
            NavigateTo(ViewType.Settings);
        }

        private void OnGoToLogs()
        {
            NavigateTo(ViewType.Logs);
        }

        private void OnGoToAbout()
        {
            NavigateTo(ViewType.About);
        }

        private async void OnLogout()
        {
            if (_settingsService.Settings.User != null)
                await _settingsService.SetSettingsAsync(null);

            MessengerInstance.Send<User>(null);
        }

        private void OnRequestHelp()
        {
            Process.Start($"mailto:{_emailSettings.SupportEmailAddress}?subject={_emailSettings.SupportEmailSubject}&Attach={_loggerService.TodayLogsFilePath}");
        }
        #endregion
    }
}