using AutoPrintr.Helpers;
using AutoPrintr.IServices;
using AutoPrintr.Models;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;

namespace AutoPrintr.ViewModels
{
    public class TrayIconContextMenuViewModel : BaseViewModel
    {
        #region Properties
        private readonly ISettingsService _settingsService;

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
        public RelayCommand LogoutCommand { get; private set; }
        #endregion

        #region Constructors
        public TrayIconContextMenuViewModel(INavigationService navigationService,
            ISettingsService settingsService)
            : base(navigationService)
        {
            _settingsService = settingsService;

            GoToLoginCommand = new RelayCommand(OnGoToLogin);
            GoToSettingsCommand = new RelayCommand(OnGoToSettings);
            GoToJobsCommand = new RelayCommand(OnGoToJobs);
            LogoutCommand = new RelayCommand(OnLogout);

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

        private async void OnLogout()
        {
            if (_settingsService.Settings.User != null)
            {
                _settingsService.Settings.User = null;
                await _settingsService.SaveSettingsAsync();
            }

            MessengerInstance.Send<User>(null);
        }
        #endregion
    }
}