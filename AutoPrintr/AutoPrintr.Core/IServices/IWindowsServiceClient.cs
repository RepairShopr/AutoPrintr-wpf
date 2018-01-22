using AutoPrintr.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoPrintr.Core.IServices
{
    public interface IWindowsServiceClient
    {
        Action<Job> JobChangedAction { get; set; }
        Task ConnectAsync(Action connectionFailed);
        Task DisconnectAsync();
        Task<IEnumerable<Printer>> GetPrintersAsync();
        Task<IEnumerable<Job>> GetJobsAsync();
        Task<bool> PrintAsync(Job job);
        Task<bool> DeleteJobsAsync(Job[] jobs);
    }
}