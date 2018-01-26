using AutoPrintr.Core.Models;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace AutoPrintr.Core.IServices
{
    [ServiceContract(Name = nameof(IWindowsService), SessionMode = SessionMode.Required, CallbackContract = typeof(IWindowsServiceCallback))]
    public interface IWindowsService
    {
        [OperationContract]
        void Connect();

        [OperationContract]
        void Disconnect();

        [OperationContract]
        void Ping();

        [OperationContract]
        Task<IEnumerable<Printer>> GetPrinters();

        [OperationContract]
        Task<IEnumerable<Job>> GetJobs();

        [OperationContract]
        void Print(Job job);

        [OperationContract]
        void DeleteJobs(IEnumerable<Job> jobs);
    }
}