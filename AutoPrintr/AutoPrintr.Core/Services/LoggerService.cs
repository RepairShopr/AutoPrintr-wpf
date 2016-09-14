using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPrintr.Core.Services
{
    internal class LoggerService : ILoggerService
    {
        #region Properties
        private readonly IFileService _fileService;

        private ICollection<Log> _logs;
        public IEnumerable<Log> Logs => _logs;
        public string TodayLogsFilePath => _fileService.GetFilePath(GetLogFileName(DateTime.Now));
        #endregion

        #region Constructors
        public LoggerService(IFileService fileService)
        {
            _fileService = fileService;

            _logs = new List<Log>();

            GetLogsAsync();
        }
        #endregion

        #region Methods
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

        private async void GetLogsAsync()
        {
            var existingLogs = await _fileService.ReadObjectAsync<ICollection<Log>>(GetLogFileName(DateTime.Now));
            if (existingLogs != null)
                _logs = existingLogs.Union(_logs).ToList();
        }

        private void AddLog(string message, LogType type)
        {
            var newLogItem = new Log { DateTime = DateTime.Now, Event = message, Type = type };
            _logs.Add(newLogItem);
        }

        private async void SaveLogsAsync()
        {
            await _fileService.SaveObjectAsync(GetLogFileName(DateTime.Now), _logs);
        }

        private string GetLogFileName(DateTime date)
        {
            return $"Logs/{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}_Logs.json";
        }
        #endregion
    }
}