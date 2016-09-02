using AutoPrintr.Helpers;
using AutoPrintr.IServices;
using AutoPrintr.Models;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;

namespace AutoPrintr.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        #region Properties
        private readonly ISettingsService _settingsService;
        private readonly IUserService _userService;

        public override ViewType Type => ViewType.Login;

        private Login _login;
        public Login Login
        {
            get { return _login; }
            private set { Set(ref _login, value); }
        }

        private bool _rememberMe;
        public bool RememberMe
        {
            get { return _rememberMe; }
            set { Set(ref _rememberMe, value); }
        }

        public RelayCommand LoginCommand { get; private set; }
        #endregion

        #region Constructors
        public LoginViewModel(INavigationService navigationService,
            ISettingsService settingsService,
            IUserService userService)
            : base(navigationService)
        {
            _settingsService = settingsService;
            _userService = userService;

            Login = new Login();
            LoginCommand = new RelayCommand(OnLogin);
        }
        #endregion

        #region Methods
        private async void OnLogin()
        {
            if (!Login.ValidateProperties())
            {
                ShowMessageControl(Login.GetAllErrors());
                return;
            }

            ShowBusyControl("Authenticating");

            var user = await _userService.LoginAsync(Login);
            if (user == null)
            {
                HideBusyControl();
                ShowMessageControl("Authentication failed. Incorrect username or password");
                return;
            }

            var channel = await _userService.GetChannelAsync(user);
            if (channel == null)
            {
                HideBusyControl();
                ShowMessageControl("Operation of getting channel is failed");
                return;
            }

            if (RememberMe)
                await _settingsService.SetSettingsAsync(user, channel);
            else
                await _settingsService.SetSettingsAsync(null, channel);

            HideBusyControl();

            MessengerInstance.Send(user);
            NavigateTo(ViewType.Settings);
        }
        #endregion
    }
}