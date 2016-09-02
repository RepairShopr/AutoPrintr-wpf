using AutoPrintr.Helpers;
using AutoPrintr.Models;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;

namespace AutoPrintr.ViewModels
{
    public class TrayIconContextMenuViewModel : BaseViewModel
    {
        #region Properties
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
        #endregion

        #region Constructors
        public TrayIconContextMenuViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            GoToLoginCommand = new RelayCommand(OnGoToLogin);
            GoToSettingsCommand = new RelayCommand(OnGoToSettings);
            GoToJobsCommand = new RelayCommand(OnGoToJobs);

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
        #endregion
    }
}