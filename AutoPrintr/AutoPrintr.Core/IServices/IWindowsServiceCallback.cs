using AutoPrintr.Core.Models;
using System.ServiceModel;

namespace AutoPrintr.Core.IServices
{
    public interface IWindowsServiceCallback
    {
        [OperationContract]
        void JobChanged(Job job);
    }
}