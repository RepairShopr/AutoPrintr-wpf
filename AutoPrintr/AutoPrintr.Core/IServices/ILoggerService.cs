using AutoPrintr.Core.Models;
using System;
using System.Collections.Generic;

namespace AutoPrintr.Core.IServices
{
    public interface ILoggerService
    {
        IEnumerable<Log> Logs { get; }
        string TodayLogsFilePath { get; }
        void WriteInformation(string message);
        void WriteWarning(string message);
        void WriteError(string message);
        void WriteError(Exception exception);
    }
}