using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Services;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using System.Threading.Tasks;

namespace AutoPrintr.Core
{
    public abstract class App
    {
        #region Constructors
        static App()
        { }

        protected App()
        { }
        #endregion

        #region Methods
        public virtual async Task Startup()
        {
            RegisterTypes();
            await InitializeLogsAsync();
            await LoadSettingsAsync();
        }

        public virtual async Task RunJobs()
        {
            var jobsService = SimpleIoc.Default.GetInstance<IJobsService>();
            await jobsService.RunAsync();
        }

        public virtual async Task StopJobs()
        {
            var jobsService = SimpleIoc.Default.GetInstance<IJobsService>();
            await jobsService.StopAsync();
        }

        protected virtual void RegisterTypes()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //Register Services
            SimpleIoc.Default.Register<ILoggerService, LoggerService>();
            SimpleIoc.Default.Register<IApiService, ApiService>();
            SimpleIoc.Default.Register<IFileService, FileService>();
            SimpleIoc.Default.Register<ISettingsService, SettingsService>();
            SimpleIoc.Default.Register<IUserService, UserService>();
            SimpleIoc.Default.Register<IPrinterService, PrinterService>();
            SimpleIoc.Default.Register<IJobsService, JobsService>();
        }

        protected virtual async Task<bool> LoadSettingsAsync()
        {
            var settingsService = SimpleIoc.Default.GetInstance<ISettingsService>();
            return await settingsService.LoadSettingsAsync();
        }

        protected abstract Task InitializeLogsAsync();
        #endregion
    }
}