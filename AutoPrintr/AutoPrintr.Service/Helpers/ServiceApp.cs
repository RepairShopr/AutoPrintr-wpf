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
    }
}