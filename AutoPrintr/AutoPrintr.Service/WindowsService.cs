using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using AutoPrintr.Service.IServices;
using GalaSoft.MvvmLight.Ioc;
using System.Collections.Generic;
using System.ServiceModel;

namespace AutoPrintr.Service
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    internal class WindowsService : IWindowsService
    {
        #region Properties
        private readonly static ICollection<IWindowsServiceCallback> _callBackList = new List<IWindowsServiceCallback>();

        private IJobsService JobsService => SimpleIoc.Default.GetInstance<IJobsService>();
        private IPrinterService PrintersService => SimpleIoc.Default.GetInstance<IPrinterService>();
        #endregion

        #region Methods
        public void Connect()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IWindowsServiceCallback>();
            if (!_callBackList.Contains(callback))
                _callBackList.Add(callback);
        }

        public void Disconnect()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IWindowsServiceCallback>();
            if (_callBackList.Contains(callback))
                _callBackList.Remove(callback);
        }

        public IEnumerable<Printer> GetPrinters()
        {
            var task = PrintersService.GetPrintersAsync();
            task.Wait();
            return task.Result;
        }

        public IEnumerable<Job> GetJobs()
        {
            return JobsService.GetJobs();
        }

        public void Print(Job job)
        {
            JobsService.Print(job);
        }

        public void DeleteJobs(IEnumerable<Job> jobs)
        {
            JobsService.DeleteJobs(jobs);
        }

        public void JobChanged(Job job)
        {
            foreach (var callback in _callBackList)
                callback.JobChanged(job);
        }

        public void ConnectionFailed()
        {
            foreach (var callback in _callBackList)
                callback.ConnectionFailed();
        }
        #endregion
    }
}