using AutoPrintr.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoPrintr.IServices
{
    public interface IJobsService
    {
        IEnumerable<Job> Jobs { get; }
        Task RunAsync();
        Task StopAsync();
    }
}