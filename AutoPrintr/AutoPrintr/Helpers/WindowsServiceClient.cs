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
        private Action _connectionFailed;
        private WindowsServiceReference.WindowsServiceClient _windowsServiceClient;

        public bool Connected => _windowsServiceClient?.State == CommunicationState.Opened;

        public async Task ConnectAsync(Action<Job> jobChanged)
        {
            _jobChanged = jobChanged;

            try
            {
                var instanceContext = new InstanceContext(this);
                _windowsServiceClient = new WindowsServiceReference.WindowsServiceClient(instanceContext);

                await Task.Run(() =>
                {
                    try
                    {
                        _windowsServiceClient.Open();
                    }
                    catch (EndpointNotFoundException)
                    { }
                });

                await _windowsServiceClient.ConnectAsync();
            }
            catch (CommunicationObjectFaultedException)
            { }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                await _windowsServiceClient.DisconnectAsync();
                _windowsServiceClient.Close();
            }
            catch (CommunicationObjectFaultedException)
            { }
        }

        public async Task<IEnumerable<Printer>> GetPrintersAsync()
        {
            try
            {
                var instanceContext = new InstanceContext(this);
                using (var windowsServiceClient = new WindowsServiceReference.WindowsServiceClient(instanceContext))
                {
                    return await windowsServiceClient.GetPrintersAsync();
                }
            }
            catch (CommunicationObjectFaultedException)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Job>> GetJobsAsync()
        {
            try
            {
                return await _windowsServiceClient.GetJobsAsync();
            }
            catch (CommunicationObjectFaultedException)
            {
                return null;
            }
        }

        public async Task<bool> PrintAsync(Job job)
        {
            try
            {
                await _windowsServiceClient.PrintAsync(job);
                return true;
            }
            catch (CommunicationObjectFaultedException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteJobsAsync(Job[] jobs)
        {
            try
            {
                await _windowsServiceClient.DeleteJobsAsync(jobs);
                return true;
            }
            catch (CommunicationObjectFaultedException)
            {
                return false;
            }
        }

        public void JobChanged(Job job)
        {
            _jobChanged?.Invoke(job);
        }

        public void ConnectionFailed()
        {
            _connectionFailed?.Invoke();
        }
    }
}