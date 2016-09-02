using AutoPrintr.Helpers;
using AutoPrintr.IServices;
using GalaSoft.MvvmLight.Views;

namespace AutoPrintr.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        #region Properties
        private readonly ISettingsService _settingsService;

        public override ViewType Type => ViewType.Settings;
        #endregion

        #region Constructors
        public SettingsViewModel(INavigationService navigationService,
            ISettingsService settingsService)
            : base(navigationService)
        {
            _settingsService = settingsService;
        }
        #endregion

        #region Methods

        #endregion
    }
}