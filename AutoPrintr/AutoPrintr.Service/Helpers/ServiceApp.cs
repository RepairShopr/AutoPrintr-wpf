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
        private static readonly ServiceApp _instance = new ServiceApp();

        public static ServiceApp Instance => _instance;
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

            SimpleIoc.Default.Register<IJobsService, JobsService>();
        }

        protected override async Task<bool> LoadSettingsAsync()
        {
            var result = await base.LoadSettingsAsync();

            var settingsService = SimpleIoc.Default.GetInstance<ISettingsService>();
            settingsService.MonitorSettingsChanges();

            return result;
        }

        protected override Task InitializeLogsAsync()
        {
            var loggerService = SimpleIoc.Default.GetInstance<ILoggerService>();
            return loggerService.InitializeServiceLogsAsync();
        }

        #region Jobs
        public async Task RunJobs(Action<Job> jobChanged)
        {
            _jobChanged = jobChanged;

            var jobsService = SimpleIoc.Default.GetInstance<IJobsService>();
            jobsService.JobChangedEvent -= JobsService_JobChangedEvent;
            jobsService.JobChangedEvent += JobsService_JobChangedEvent;
            await jobsService.RunAsync();
        }

        private void JobsService_JobChangedEvent(Job job)
        {
            _jobChanged?.Invoke(job);
        }

        public async Task StopJobs()
        {
            var jobsService = SimpleIoc.Default.GetInstance<IJobsService>();
            await jobsService.StopAsync();
            jobsService.JobChangedEvent -= JobsService_JobChangedEvent;
        }
        #endregion

        #endregion
    }
}