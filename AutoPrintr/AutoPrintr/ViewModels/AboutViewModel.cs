using AutoPrintr.Helpers;
using GalaSoft.MvvmLight.Views;

namespace AutoPrintr.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        #region Properties
        public string Licence { get; private set; }
        public string About { get; private set; }

        public override ViewType Type => ViewType.About;
        #endregion

        #region Constructors
        public AboutViewModel(INavigationService navigationService)
            : base(navigationService)
        { }
        #endregion

        #region Methods
        public override void NavigatedTo(object parameter = null)
        {
            base.NavigatedTo(parameter);

            Licence = Resources.License;
            About = Resources.About;
        }
        #endregion
    }
}