using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using AutoPrintr.Service.IServices;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static ISettingsService SettingsService => SimpleIoc.Default.GetInstance<ISettingsService>();
        private static ILoggerService LoggerService => SimpleIoc.Default.GetInstance<ILoggerService>();

        private static ServiceHost _serviceHost;
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

        public void Ping()
        { }

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
            {
                try
                {
                    callback.JobChanged(job);
                }
                catch (CommunicationException ex)
                {
                    _callBackList.Remove(callback);

                    Debug.WriteLine($"Error in {nameof(WindowsService)}: {ex.ToString()}");
                    LoggerService.WriteWarning($"Error in {nameof(WindowsService)}: {ex.ToString()}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in {nameof(WindowsService)}: {ex.ToString()}");
                    LoggerService.WriteError($"Error in {nameof(WindowsService)}: {ex.ToString()}");
                }
            }
        }

        public void ConnectionFailed()
        {
            foreach (var callback in _callBackList)
            {
                try
                {
                    callback.ConnectionFailed();
                }
                catch (CommunicationException ex)
                {
                    _callBackList.Remove(callback);

                    Debug.WriteLine($"Error in {nameof(WindowsService)}: {ex.ToString()}");
                    LoggerService.WriteWarning($"Error in {nameof(WindowsService)}: {ex.ToString()}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in {nameof(WindowsService)}: {ex.ToString()}");
                    LoggerService.WriteError($"Error in {nameof(WindowsService)}: {ex.ToString()}");
                }
            }
        }
        #endregion

        #region Static Methods
        public static async void StartServiceHost()
        {
            try
            {
                _serviceHost = CreateServiceHost();
                _serviceHost.Open();
            }
            catch (AddressAlreadyInUseException)
            {
                var newPortNumber = SettingsService.Settings.PortNumber + 1;
                LoggerService.WriteWarning($"Port number {SettingsService.Settings.PortNumber} is busy. Changing it to: {newPortNumber}");
                SettingsService.Settings.PortNumber = newPortNumber;

                StartServiceHost();
                await SettingsService.UpdateSettingsAsync(newPortNumber);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsService)}: {ex.ToString()}");
                LoggerService.WriteError(ex);
            }
        }

        public static void OnJobChanged(Job job)
        {
            try
            {
                if (_serviceHost.State == CommunicationState.Opened)
                    ((WindowsService)_serviceHost.SingletonInstance).JobChanged(job);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsService)}: {ex.ToString()}");
                LoggerService.WriteError(ex);
            }
        }

        public static void OnConnectionFailed()
        {
            try
            {
                if (_serviceHost.State == CommunicationState.Opened)
                    ((WindowsService)_serviceHost.SingletonInstance).ConnectionFailed();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsService)}: {ex.ToString()}");
                LoggerService.WriteError(ex);
            }
        }

        public static void StopServiceHost()
        {
            try
            {
                _serviceHost.Close();
                _serviceHost = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsService)}: {ex.ToString()}");
                LoggerService.WriteError(ex);
            }
        }

        private static ServiceHost CreateServiceHost()
        {
            var service = new WindowsService();
            var serviceHost = new ServiceHost(service, GetServiceAddress());

            return serviceHost;
        }

        private static Uri GetServiceAddress()
        {
            return new Uri($"net.tcp://localhost:{SettingsService.Settings.PortNumber}/AutoPrintrService");
        }
        #endregion
    }
}