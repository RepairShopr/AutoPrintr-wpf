using AutoPrintr.IServices;
using AutoPrintr.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AutoPrintr.Services
{
    internal class LoggerService : ILoggerService
    {
        #region Properties
        private readonly string _source;
        #endregion

        #region Constructors
        public LoggerService()
        {
            _source = Assembly.GetEntryAssembly().GetName().Name;

            if (!EventLog.SourceExists(_source))
                EventLog.CreateEventSource(_source, _source);
        }
        #endregion

        #region Methods
        public async Task<IEnumerable<Log>> GetLogsAsync()
        {
            return await Task.Factory.StartNew<IEnumerable<Log>>(() =>
            {
                using (var logger = new EventLog(_source, Environment.MachineName, _source))
                {
                    return logger.Entries
                        .OfType<EventLogEntry>()
                        .Select(x => new Log { DateTime = x.TimeWritten, Event = x.Message })
                        .OrderByDescending(x => x.DateTime)
                        .ToList();
                }
            });
        }

        public void WriteInformation(string message)
        {
            Task.Factory.StartNew(() =>
            {
                EventLog.WriteEntry(_source, message, EventLogEntryType.Information);
            });
        }

        public void WriteWarning(string message)
        {
            Task.Factory.StartNew(() =>
            {
                EventLog.WriteEntry(_source, message, EventLogEntryType.Warning);
            });
        }

        public void WriteError(string message)
        {
            Task.Factory.StartNew(() =>
            {
                EventLog.WriteEntry(_source, message, EventLogEntryType.Error);
            });
        }

        public void WriteError(Exception exception)
        {
            Task.Factory.StartNew(() =>
            {
                EventLog.WriteEntry(_source, exception.ToString(), EventLogEntryType.Error);
            });
        }
        #endregion
    }
}