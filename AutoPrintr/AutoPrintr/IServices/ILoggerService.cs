using System;

namespace AutoPrintr.IServices
{
    public interface ILoggerService
    {
        void WriteInformation(string message);
        void WriteWarning(string message);
        void WriteError(string message);
        void WriteError(Exception exception);
    }
}