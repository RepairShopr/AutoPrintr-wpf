using AutoPrintr.Helpers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;

namespace AutoPrintr.ViewModels
{
    public abstract class BaseViewModel : ViewModelBase
    {
        #region Properties
        private readonly INavigationService _navigationService;

        public abstract ViewType Type { get; }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { Set(ref _isBusy, value); }
        }
        #endregion

        #region Constructors
        public BaseViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }
        #endregion

        #region Methods
        public virtual void GoBack()
        {
            _navigationService.GoBack();
        }

        public virtual void NavigateTo(ViewType view, object parm = null)
        {
            if (parm == null)
                _navigationService.NavigateTo(view.ToString());
            else
                _navigationService.NavigateTo(view.ToString(), parm);
        }

        public virtual void NavigatedTo(object parameter = null)
        { }
        #endregion
    }
}