using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using AutoPrintr.Service.IServices;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;

namespace AutoPrintr.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal class WindowsService : IWindowsService
    {
        #region Properties
        private readonly List<IWindowsServiceCallback> _callbacks = new List<IWindowsServiceCallback>();
        private readonly object guardCallbacks = new object();

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
            if (callback == null)
            {
                return;
            }

            lock (guardCallbacks)
            {
                _callbacks.Add(callback);
            }
        }

        public void Disconnect()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IWindowsServiceCallback>();
            if (callback == null)
            {
                return;
            }

            lock (guardCallbacks)
            {
                if (_callbacks.Contains(callback))
                    _callbacks.Remove(callback);
            }
        }

        public void Ping()
        { }

        public Task<IEnumerable<Printer>> GetPrinters()
        {
            return Task.FromResult(PrintersService.GetPrinters());
        }

        public Task<IEnumerable<Job>> GetJobs()
        {
            return Task.FromResult(JobsService.GetJobs());
        }

        public void Print(Job job)
        {
            JobsService.Print(job);
        }

        public void DeleteJobs(IEnumerable<Job> jobs)
        {
            JobsService.DeleteJobs(jobs);
        }

        public void JobChanged(Job job) =>
            ForEachCallback(callback => callback.JobChanged(job));

        public void ConnectionFailed() =>
            ForEachCallback(callback => callback.ConnectionFailed());
        #endregion

        #region Static Methods
        public static void StartServiceHost()
        {
            try
            {
                var service = new WindowsService();
                _serviceHost = new ServiceHost(service);
                _serviceHost.Open();
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
                if (_serviceHost != null)
                {
                    _serviceHost.Close();
                    _serviceHost = null;
                }
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

        private void ForEachCallback(Action<IWindowsServiceCallback> call, bool store = false)
        {
            lock (guardCallbacks)
            {
                for (int i = _callbacks.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        call(_callbacks[i]);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        _callbacks.RemoveAt(i);
                        Debug.WriteLine($"Error in {nameof(WindowsService)}: {ex.Message}");
                        LoggerService.WriteWarning($"Error in {nameof(WindowsService)}: {ex.Message}");
                    }
                    catch (CommunicationException ex)
                    {
                        _callbacks.RemoveAt(i);
                        Debug.WriteLine($"Error in {nameof(WindowsService)}: {ex.Message}");
                        LoggerService.WriteWarning($"Error in {nameof(WindowsService)}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in {nameof(WindowsService)}: {ex}");
                        LoggerService.WriteError($"Error in {nameof(WindowsService)}: {ex}");
                    }
                }
            }
        }

        #endregion
    }
}