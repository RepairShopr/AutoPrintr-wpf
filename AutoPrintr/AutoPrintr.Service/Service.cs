using AutoPrintr.Core.IServices;
using AutoPrintr.Service.Helpers;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace AutoPrintr.Service
{
    internal class Service : ServiceBase
    {
        #region Properties
        public const string SERVICE_NAME = "AutoPrintr Service";
        #endregion

        #region Constructors
        public Service()
        {
            ServiceName = SERVICE_NAME;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        #endregion

        #region Methods
        protected override async void OnStart(string[] args)
        {
            base.OnStart(args);

            await ServiceApp.Instance.Startup(args);
            await ServiceApp.Instance.RunJobs(WindowsService.OnConnectionFailed, WindowsService.OnJobChanged);
            WindowsService.StartServiceHost();
        }

        protected override async void OnStop()
        {
            base.OnStop();

            await ServiceApp.Instance.StopJobs();
            WindowsService.StopServiceHost();
        }
        #endregion

        #region Static Methods
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (Environment.UserInteractive)
            {
                foreach (var commandString in args)
                {
                    var items = commandString.Split('/');
                    foreach (var item in items)
                    {
                        Commands command;
                        if (Enum.TryParse(item, out command))
                            RunCommand(command);
                    }
                }
            }
            else
            {
                Run(new Service());
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Error in {nameof(Service)}: {((Exception)e.ExceptionObject).ToString()}");

            var loggingService = SimpleIoc.Default.GetInstance<ILoggerService>();
            loggingService.WriteError((Exception)e.ExceptionObject);
        }

        private static void RunCommand(Commands command)
        {
            switch (command)
            {
                case Commands.Install:
                    if (!ServiceController.GetServices().Any(x => x.ServiceName == SERVICE_NAME))
                        ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                    break;
                case Commands.Start:
                    using (ServiceController sc = new ServiceController(SERVICE_NAME))
                    {
                        if (ServiceController.GetServices().Any(x => x.ServiceName == SERVICE_NAME))
                        {
                            if (sc.Status != ServiceControllerStatus.Running)
                                sc.Start();
                        }
                    }
                    break;
                case Commands.Stop:
                    using (var sc = new ServiceController(SERVICE_NAME))
                    {
                        if (ServiceController.GetServices().Any(x => x.ServiceName == SERVICE_NAME))
                        {
                            if (sc.Status != ServiceControllerStatus.Stopped)
                                sc.Stop();
                        }
                    }
                    break;
                case Commands.Uninstall:
                    if (ServiceController.GetServices().Any(x => x.ServiceName == SERVICE_NAME))
                        ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                    break;
                case Commands.Debug:
                    var service = new Service();
                    service.OnStart(new string[0]);
                    Thread.Sleep(Timeout.Infinite);
                    break;
            }
        }
        #endregion
    }
}