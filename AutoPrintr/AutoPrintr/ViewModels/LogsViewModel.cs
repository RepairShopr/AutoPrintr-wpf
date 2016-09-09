using AutoPrintr.Helpers;
using AutoPrintr.IServices;
using AutoPrintr.Models;
using GalaSoft.MvvmLight.Views;
using System.Collections.Generic;

namespace AutoPrintr.ViewModels
{
    public class LogsViewModel : BaseViewModel
    {
        #region Properties
        private readonly ILoggerService _logsService;

        public IEnumerable<Log> Logs { get; private set; }

        public override ViewType Type => ViewType.Logs;
        #endregion

        #region Constructors
        public LogsViewModel(INavigationService navigationService,
            ILoggerService logsService)
            : base(navigationService)
        {
            _logsService = logsService;
        }
        #endregion

        #region Methods
        public override async void NavigatedTo(object parameter = null)
        {
            base.NavigatedTo(parameter);
            Logs = null;

            ShowBusyControl();

            Logs = await _logsService.GetLogsAsync();
            RaisePropertyChanged(nameof(Logs));

            HideBusyControl();
        }
        #endregion
    }
}