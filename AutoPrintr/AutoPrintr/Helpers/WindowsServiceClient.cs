using AutoPrintr.Core.Models;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace AutoPrintr.Helpers
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Single, UseSynchronizationContext = false)]
    internal class WindowsServiceClient : WindowsServiceReference.IWindowsServiceCallback
    {
        private Action<Job> _jobChanged;
        private WindowsServiceReference.WindowsServiceClient _windowsServiceClient;

        public async Task ConnectAsync(Action<Job> jobChanged)
        {
            _jobChanged = jobChanged;

            var instanceContext = new InstanceContext(this);
            _windowsServiceClient = new WindowsServiceReference.WindowsServiceClient(instanceContext);

            _windowsServiceClient.Open();
            await _windowsServiceClient.ConnectAsync();
        }

        public async Task DisconnectAsync()
        {
            await _windowsServiceClient.DisconnectAsync();
            _windowsServiceClient.Close();
        }

        public async Task<IEnumerable<Printer>> GetPrintersAsync()
        {
            var instanceContext = new InstanceContext(this);
            using (var windowsServiceClient = new WindowsServiceReference.WindowsServiceClient(instanceContext))
            {
                return await windowsServiceClient.GetPrintersAsync();
            }
        }

        public async Task<IEnumerable<Job>> GetJobsAsync()
        {
            return await _windowsServiceClient.GetJobsAsync();
        }

        public async Task PrintAsync(Job job)
        {
            await _windowsServiceClient.PrintAsync(job);
        }

        public async Task DeleteJobsAsync(Job[] jobs)
        {
            await _windowsServiceClient.DeleteJobsAsync(jobs);
        }

        public void JobChanged(Job job)
        {
            _jobChanged?.Invoke(job);
        }
    }
}