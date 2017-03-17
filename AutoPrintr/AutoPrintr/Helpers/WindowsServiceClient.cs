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
    internal class WindowsServiceClient : WindowsServiceReference.IWindowsServiceCallback, IWindowsServiceClient
    {
        #region Properties
        private const int PING_INTERVAL_MINUTES = 60;

        private readonly ISettingsService _settingsService;
        private readonly ILoggerService _loggerService;
        private readonly System.Timers.Timer _timer;

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

            _timer = new System.Timers.Timer(TimeSpan.FromMinutes(PING_INTERVAL_MINUTES).TotalMilliseconds);
            _timer.Elapsed += async (s, e) => { await Ping(); };
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
                        _timer.Start();
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
            catch (CommunicationException ex)
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
                        _timer.Stop();
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
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");

                _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
            }
        }

        public async Task Ping()
        {
            try
            {
                if (!Connected)
                    return;

                await _windowsServiceClient.PingAsync();
            }
            catch (CommunicationException ex)
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
            catch (CommunicationException ex)
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
            catch (CommunicationException ex)
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
            catch (CommunicationException ex)
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
            catch (CommunicationException ex)
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