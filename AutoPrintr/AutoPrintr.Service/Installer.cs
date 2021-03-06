﻿using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace AutoPrintr.Service
{
    [RunInstaller(true)]
    public class Installer : System.Configuration.Install.Installer
    {
        public Installer()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.DisplayName = Service.SERVICE_NAME;
            serviceInstaller.Description = Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes(false)
                .OfType<AssemblyDescriptionAttribute>()
                .Select(x => x.Description)
                .First();
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            serviceInstaller.ServiceName = Service.SERVICE_NAME;
            this.Installers.Add(processInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}