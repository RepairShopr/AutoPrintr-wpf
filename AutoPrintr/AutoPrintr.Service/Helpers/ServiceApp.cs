using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using AutoPrintr.Service.IServices;
using AutoPrintr.Service.Services;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Threading.Tasks;

namespace AutoPrintr.Service.Helpers
{
    internal class ServiceApp : Core.App
    {
        #region Properties
        private Action<Job> _jobChanged;
        private Action _connectionFailed;
        private static readonly ServiceApp _instance = new ServiceApp();

        public static ServiceApp Instance => _instance;

        private ILoggerService LoggerService => SimpleIoc.Default.GetInstance<ILoggerService>();
        private ISettingsService SettingsService => SimpleIoc.Default.GetInstance<ISettingsService>();
        private IJobsService JobsService => SimpleIoc.Default.GetInstance<IJobsService>();
        #endregion

        #region Constructors
        static ServiceApp()
        { }

        private ServiceApp()
        { }
        #endregion

        #region Methods    
        protected override void RegisterTypes()
        {
            base.RegisterTypes();

            SimpleIoc.Default.Register<IPrinterService, PrinterService>();
            SimpleIoc.Default.Register<IJobsService, JobsService>();
        }

        protected override async Task<bool> LoadSettingsAsync()
        {
            var result = await base.LoadSettingsAsync();

            SettingsService.MonitorSettingsChanges();

            return result;
        }

        protected override Task InitializeLogsAsync()
        {
            return LoggerService.InitializeServiceLogsAsync();
        }

        #region Jobs
        public async Task RunJobs(Action connectionFailed, Action<Job> jobChanged)
        {
            _jobChanged = jobChanged;
            _connectionFailed = connectionFailed;

            JobsService.JobChangedEvent -= JobsService_JobChangedEvent;
            JobsService.JobChangedEvent += JobsService_JobChangedEvent;

            JobsService.ConnectionFailedEvent -= JobsService_ConnectionFailedEvent;
            JobsService.ConnectionFailedEvent += JobsService_ConnectionFailedEvent;

            await JobsService.RunAsync();
        }

        private void JobsService_ConnectionFailedEvent()
        {
            _connectionFailed?.Invoke();
        }

        private void JobsService_JobChangedEvent(Job job)
        {
            _jobChanged?.Invoke(job);
        }

        public async Task StopJobs()
        {
            JobsService.JobChangedEvent -= JobsService_JobChangedEvent;
            JobsService.ConnectionFailedEvent -= JobsService_ConnectionFailedEvent;

            await JobsService.StopAsync();
        }
        #endregion

        #endregion
    }
}