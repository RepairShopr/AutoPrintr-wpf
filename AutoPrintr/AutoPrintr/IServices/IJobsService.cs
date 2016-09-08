using AutoPrintr.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoPrintr.IServices
{
    public delegate void JobChangedEventHandler(Job job);

    public interface IJobsService
    {
        event JobChangedEventHandler JobChangedEvent;

        IEnumerable<Job> Jobs { get; }
        Task RunAsync();
        void Print(Job job);
        void DeleteJob(Job job);
        void DeleteJobs(DateTime startDate, DateTime endDate);
        Task StopAsync();
    }
}