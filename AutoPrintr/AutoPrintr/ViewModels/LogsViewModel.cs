using AutoPrintr.Helpers;
using AutoPrintr.IServices;
using AutoPrintr.Models;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPrintr.ViewModels
{
    public class LogsViewModel : BaseViewModel
    {
        #region Properties
        private readonly ILoggerService _logsService;

        public IEnumerable<KeyValuePair<LogType?, string>> LogTypes { get; private set; }

        private LogType? _selectedLogType;
        public LogType? SelectedLogType
        {
            get { return _selectedLogType; }
            set { Set(ref _selectedLogType, value); LoadLogs(); }
        }

        public IEnumerable<Log> Logs { get; private set; }

        public override ViewType Type => ViewType.Logs;
        #endregion

        #region Constructors
        public LogsViewModel(INavigationService navigationService,
            ILoggerService logsService)
            : base(navigationService)
        {
            _logsService = logsService;

            LogTypes = Enum.GetValues(typeof(LogType))
                .OfType<LogType?>()
                .Union(new[] { (LogType?)null })
                .Select(x => new KeyValuePair<LogType?, string>(x, x.HasValue ? x.ToString() : "All"))
                .ToList();
        }
        #endregion

        #region Methods
        public override void NavigatedTo(object parameter = null)
        {
            base.NavigatedTo(parameter);

            Logs = null;
            _selectedLogType = null;

            LoadLogs();
        }

        private async void LoadLogs()
        {
            ShowBusyControl();

            Logs = await _logsService.GetLogsAsync(SelectedLogType);
            RaisePropertyChanged(nameof(Logs));

            HideBusyControl();
        }
        #endregion
    }
}