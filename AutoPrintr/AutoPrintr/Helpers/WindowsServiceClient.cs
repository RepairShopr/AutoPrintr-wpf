using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;

namespace AutoPrintr.Helpers
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Single, UseSynchronizationContext = false)]
    internal class WindowsServiceClient : WindowsServiceReference.IWindowsServiceCallback, Core.IServices.IWindowsServiceClient
    {
        #region Properties
        private readonly ISettingsService _settingsService;
        private readonly ILoggerService _loggerService;

        private Action _connectionFailed;
        private WindowsServiceReference.WindowsServiceClient _windowsServiceClient;

        public bool Connected => _windowsServiceClient?.State == CommunicationState.Opened;
        public Action<Job> JobChangedAction { get; set; }
        #endregion

        #region Constructors
        public WindowsServiceClient(ISettingsService settingsService,
            ILoggerService loggerService)
        {
            _settingsService = settingsService;
            _loggerService = loggerService;
        }
        #endregion

        #region Methods
        public async Task ConnectAsync(Action connectionFailed)
        {
            _connectionFailed = connectionFailed;

            _settingsService.PortNumberChangedEvent -= SettingsService_PortNumberChangedEvent;
            _settingsService.PortNumberChangedEvent += SettingsService_PortNumberChangedEvent;

            try
            {
                var instanceContext = new InstanceContext(this);
                _windowsServiceClient = new WindowsServiceReference.WindowsServiceClient(instanceContext, "NetTcpBindingEndpoint", GetServiceAddress());

                await Task.Run(() =>
                {
                    try
                    {
                        _windowsServiceClient.Open();
                    }
                    catch (TimeoutException ex)
                    {
                        Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                        _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                    }
                    catch (EndpointNotFoundException ex)
                    {
                        Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                        _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                    }
                });

                if (Connected)
                    await _windowsServiceClient.ConnectAsync();
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (!Connected)
                    return;

                await _windowsServiceClient.DisconnectAsync();

                await Task.Run(() =>
                {
                    try
                    {
                        _windowsServiceClient.Close();
                    }
                    catch (TimeoutException ex)
                    {
                        Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                        _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                    }
                    catch (EndpointNotFoundException ex)
                    {
                        Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                        _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                    }
                });

                _connectionFailed = null;
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
            }
        }

        public async Task<IEnumerable<Printer>> GetPrintersAsync()
        {
            try
            {
                if (!Connected)
                    return null;

                return await _windowsServiceClient.GetPrintersAsync();
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                return null;
            }
        }

        public async Task<IEnumerable<Job>> GetJobsAsync()
        {
            try
            {
                if (!Connected)
                    return null;

                return await _windowsServiceClient.GetJobsAsync();
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                return null;
            }
        }

        public async Task<bool> PrintAsync(Job job)
        {
            try
            {
                if (!Connected)
                    return false;

                await _windowsServiceClient.PrintAsync(job);
                return true;
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                return false;
            }
        }

        public async Task<bool> DeleteJobsAsync(Job[] jobs)
        {
            try
            {
                if (!Connected)
                    return false;

                await _windowsServiceClient.DeleteJobsAsync(jobs);
                return true;
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                return false;
            }
        }

        public void JobChanged(Job job)
        {
            JobChangedAction?.Invoke(job);
        }

        public void ConnectionFailed()
        {
            _connectionFailed?.Invoke();
        }

        private EndpointAddress GetServiceAddress()
        {
            return new EndpointAddress($"net.tcp://localhost:{_settingsService.Settings.PortNumber}/AutoPrintrService");
        }

        private async void SettingsService_PortNumberChangedEvent(int newPortNumber)
        {
            var localConnectionFailed = _connectionFailed;
            await DisconnectAsync();

            _loggerService.WriteWarning($"Port number {_settingsService.Settings.PortNumber} is changed to: {newPortNumber}");
            _settingsService.Settings.PortNumber = newPortNumber;

            await ConnectAsync(localConnectionFailed);
        }
        #endregion
    }
}