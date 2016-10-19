using AutoPrintr.Core.IServices;
using GalaSoft.MvvmLight.Ioc;
using System.Threading.Tasks;

namespace AutoPrintr.Service.Helpers
{
    internal class ServiceApp : Core.App
    {
        #region Properties
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
        #endregion
    }
}