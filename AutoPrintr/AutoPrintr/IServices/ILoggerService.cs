using AutoPrintr.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoPrintr.IServices
{
    public interface ILoggerService
    {
        Task<IEnumerable<Log>> GetLogsAsync();
        void WriteInformation(string message);
        void WriteWarning(string message);
        void WriteError(string message);
        void WriteError(Exception exception);
    }
}