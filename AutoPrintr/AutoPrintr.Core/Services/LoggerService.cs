using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPrintr.Core.Services
{
    internal class LoggerService : ILoggerService
    {
        private enum AppType
        {
            App,
            Service
        }

        #region Properties
        private readonly IFileService _fileService;

        private AppType _appType;
        private ICollection<Log> _logs;

        public string TodayLogsFilePath => _fileService.GetFilePath(GetLogFileName(DateTime.Now, _appType));
        #endregion

        #region Constructors
        public LoggerService(IFileService fileService)
        {
            _fileService = fileService;

            _logs = new List<Log>();
        }
        #endregion

        #region Methods
        public async Task InitializeAppLogsAsync()
        {
            _appType = AppType.App;

            var fileLogs = await _fileService.ReadObjectAsync<ICollection<Log>>(GetLogFileName(DateTime.Now, _appType));
            if (fileLogs != null)
                _logs = fileLogs.Union(_logs).ToList();
        }

        public async Task InitializeServiceLogsAsync()
        {
            _appType = AppType.Service;

            var fileLogs = await _fileService.ReadObjectAsync<ICollection<Log>>(GetLogFileName(DateTime.Now, _appType));
            if (fileLogs != null)
                _logs = fileLogs.Union(_logs).ToList();
        }

        public async Task<IEnumerable<Log>> GetLogsAsync(DateTime day)
        {
            var logs = new List<Log>();

            var appTypes = Enum.GetValues(typeof(AppType)).OfType<AppType>();
            foreach (var type in appTypes)
            {
                var fileLogs = await _fileService.ReadObjectAsync<ICollection<Log>>(GetLogFileName(day, type));
                if (fileLogs != null)
                    logs = fileLogs.Union(logs).ToList();
            }

            return logs.OrderByDescending(x => x.DateTime).ToList();
        }

        public void WriteInformation(string message)
        {
            AddLog(message, LogType.Information);
            SaveLogsAsync();
        }

        public void WriteWarning(string message)
        {
            AddLog(message, LogType.Warning);
            SaveLogsAsync();
        }

        public void WriteError(string message)
        {
            AddLog(message, LogType.Error);
            SaveLogsAsync();
        }

        public void WriteError(Exception exception)
        {
            AddLog(exception.ToString(), LogType.Error);
            SaveLogsAsync();
        }

        private void AddLog(string message, LogType type)
        {
            var newLogItem = new Log { DateTime = DateTime.Now, Event = message, Type = type };
            _logs.Add(newLogItem);
        }

        private async void SaveLogsAsync()
        {
            _logs = _logs.Where(x => x.DateTime.Date == DateTime.Now.Date).ToList();
            await _fileService.SaveObjectAsync(GetLogFileName(DateTime.Now, _appType), _logs);
        }

        private string GetLogFileName(DateTime date, AppType type)
        {
            return $"Logs/{date.Day}_{date.Month}_{date.Year}_{type}_Logs.json";
        }
        #endregion
    }
}