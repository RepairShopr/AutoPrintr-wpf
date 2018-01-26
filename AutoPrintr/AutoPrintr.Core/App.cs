using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Services;
using GalaSoft.MvvmLight.Ioc;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;

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
        public virtual async Task Startup(string[] args)
        {
            RegisterTypes();
            await InitializeLogsAsync();
            await LoadSettingsAsync();
        }

        protected virtual void RegisterTypes()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //Register Services
            SimpleIoc.Default.Register<IAppSettings, AppSettings>();
            SimpleIoc.Default.Register<ILoggerService, LoggerService>();
            SimpleIoc.Default.Register<IApiService, ApiService>();
            SimpleIoc.Default.Register<IFileService, FileService>();
            SimpleIoc.Default.Register<ISettingsService, SettingsService>();
            SimpleIoc.Default.Register<IUserService, UserService>();
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