using AutoPrintr.Helpers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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

        protected void ShowBusyControl(string caption = null)
        {
            IsBusy = true;
            var message = new ShowControlMessage(ControlMessageType.Busy) { Caption = caption };
            MessengerInstance.Send(message);
        }

        protected void HideBusyControl()
        {
            IsBusy = false;
            MessengerInstance.Send(new HideControlMessage(ControlMessageType.Busy));
        }

        protected void ShowMessageControl(string message, string caption = null)
        {
            var msg = new ShowControlMessage(ControlMessageType.Message) { Caption = caption, Data = message };
            MessengerInstance.Send(msg);
        }

        protected void ShowMessageControl(ReadOnlyDictionary<string, ReadOnlyCollection<string>> errors, string caption = null)
        {
            var message = new StringBuilder();
            foreach (var error in errors.SelectMany(e => e.Value))
                message.AppendLine(error);

            var msg = new ShowControlMessage(ControlMessageType.Message) { Caption = caption, Data = message.ToString() };
            MessengerInstance.Send(msg);
        }

        protected void ShowWarningControl(string message, string caption = null)
        {
            var msg = new ShowControlMessage(ControlMessageType.Warning) { Caption = caption, Data = message };
            MessengerInstance.Send(msg);
        }
        #endregion
    }
}