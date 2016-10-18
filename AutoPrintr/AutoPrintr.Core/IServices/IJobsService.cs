using AutoPrintr.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoPrintr.Core.IServices
{
    public delegate void JobChangedEventHandler(Job job);

    public interface IJobsService
    {
        bool IsRunning { get; }

        event JobChangedEventHandler JobChangedEvent;

        Task<IEnumerable<Job>> GetJobs();
        Task RunAsync();
        void Print(Job job);
        Task DeleteJob(Job job);
        Task DeleteJobs(DateTime startDate, DateTime endDate);
        Task StopAsync();
    }
}