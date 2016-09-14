using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Services;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
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
            await LoadSettingsAsync();
            await RunJobsAsync();
        }

        public virtual async Task Exit()
        {
            await StopJobsAsync();
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

        private async Task LoadSettingsAsync()
        {
            var settingsService = SimpleIoc.Default.GetInstance<ISettingsService>();
            await settingsService.LoadSettingsAsync();

            Messenger.Default.Send(settingsService.Settings.User);
        }

        private async Task RunJobsAsync()
        {
            var jobsService = SimpleIoc.Default.GetInstance<IJobsService>();
            await jobsService.RunAsync();
        }

        private async Task StopJobsAsync()
        {
            var jobsService = SimpleIoc.Default.GetInstance<IJobsService>();
            await jobsService.StopAsync();
        }
        #endregion
    }
}