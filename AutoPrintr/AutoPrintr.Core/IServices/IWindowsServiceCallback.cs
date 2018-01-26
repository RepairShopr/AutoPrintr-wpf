using AutoPrintr.Core.Models;
using System.ServiceModel;

namespace AutoPrintr.Core.IServices
{
    [ServiceContract]
    public interface IWindowsServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void JobChanged(Job job);

        [OperationContract(IsOneWay = true)]
        void ConnectionFailed();
    }
}