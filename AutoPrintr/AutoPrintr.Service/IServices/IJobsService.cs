using AutoPrintr.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoPrintr.Service.IServices
{
    public delegate void JobChangedEventHandler(Job job);

    public interface IJobsService
    {
        bool IsRunning { get; }

        event JobChangedEventHandler JobChangedEvent;

        IEnumerable<Job> GetJobs();
        void Print(Job job);
        void DeleteJobs(IEnumerable<Job> jobs);
        Task RunAsync();
        Task StopAsync();
    }
}