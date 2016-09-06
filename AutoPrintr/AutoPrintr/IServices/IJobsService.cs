using AutoPrintr.Models;
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
        Task StopAsync();
    }
}