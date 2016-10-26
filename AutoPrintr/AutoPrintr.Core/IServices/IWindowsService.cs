using AutoPrintr.Core.Models;
using System.Collections.Generic;
using System.ServiceModel;

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
        IEnumerable<Printer> GetPrinters();

        [OperationContract]
        IEnumerable<Job> GetJobs();

        [OperationContract]
        void Print(Job job);

        [OperationContract]
        void DeleteJobs(IEnumerable<Job> jobs);
    }
}