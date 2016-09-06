using System.Threading.Tasks;

namespace AutoPrintr.IServices
{
    public interface IJobsService
    {
        Task RunAsync();
        Task StopAsync();
    }
}