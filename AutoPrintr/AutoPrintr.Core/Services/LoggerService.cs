using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using RollbarSharp;
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
        private readonly string _rollbarAccessToken;
        private static object _locker = new object();

        private AppType _appType;
        private ICollection<Log> _logs;

        public string TodayLogsFilePath => _fileService.GetFilePath(GetLogFileName(DateTime.Now, _appType));
        #endregion

        #region Constructors
        public LoggerService(IFileService fileService)
        {
            _fileService = fileService;

            _rollbarAccessToken = "";
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
            ReportExceptionToRollbar(exception);
        }

        private void AddLog(string message, LogType type)
        {
            lock (_locker)
            {
                var oldLogs = _logs.Where(x => x.DateTime.Date != DateTime.Now.Date).ToList();
                foreach (var oldLog in oldLogs)
                {
                    if (_logs.Contains(oldLog))
                        _logs.Remove(oldLog);
                }

                var newLogItem = new Log { DateTime = DateTime.Now, Event = message, Type = type };
                _logs.Add(newLogItem);
            }
        }

        private async void SaveLogsAsync()
        {
            var localLogs = _logs.ToList();
            await _fileService.SaveObjectAsync(GetLogFileName(DateTime.Now, _appType), localLogs);
        }

        private string GetLogFileName(DateTime date, AppType type)
        {
            return $"Logs/{date.Day}_{date.Month}_{date.Year}_{type}_Logs.json";
        }

        private async void ReportExceptionToRollbar(Exception exception)
        {
#if DEBUG
            return;
#endif

            try
            {
                var config = new Configuration(_rollbarAccessToken)
                {
                    Environment = "Production",
                    Platform = $"AutoPrintr.{_appType}"
                };
                var client = new RollbarClient(config);
                await client.SendErrorException(exception, Environment.MachineName);
            }
            catch { }
        }
        #endregion
    }
}