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
            //TODO: Add validation
            if (string.IsNullOrEmpty(Login.Email))
            {
                System.Windows.MessageBox.Show("Username is required");
                return;
            }
            if (string.IsNullOrEmpty(Login.Password))
            {
                System.Windows.MessageBox.Show("Password is required");
                return;
            }

            var user = await _userService.LoginAsync(Login);
            if (user == null)
            {
                System.Windows.MessageBox.Show("Authentication failed. Incorrect username or password");
                return;
            }

            var channel = await _userService.GetChannelAsync(user);
            if (user == null)
            {
                System.Windows.MessageBox.Show("Operation of getting channel is failed");
                return;
            }

            if (RememberMe)
                _settingsService.Settings.User = user;
            else
                _settingsService.Settings.User = null;

            _settingsService.Settings.Channel = channel;
            await _settingsService.SaveSettingsAsync();

            MessengerInstance.Send(user);
            NavigateTo(ViewType.Settings);
        }
        #endregion
    }
}