using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPrintr.Core.Services
{
    internal class SettingsService : ISettingsService
    {
        #region Properties
        private readonly IFileService _fileService;
        private readonly ILoggerService _loggingService;
        private readonly string _fileName = $"Data/{nameof(Settings)}.json";
        private static object _locker = new object();

        public event ChannelChangedEventHandler ChannelChangedEvent;
        public event PortNumberChangedEventHandler PortNumberChangedEvent;

        private FileSystemWatcher _watcher;

        public Settings Settings { get; private set; }
        #endregion

        #region Constructors
        public SettingsService(IFileService fileService,
            ILoggerService loggingService)
        {
            _fileService = fileService;
            _loggingService = loggingService;
        }
        #endregion

        #region Methods
        public async Task<bool> LoadSettingsAsync()
        {
            _loggingService.WriteInformation("Starting load settings");

            Settings = await _fileService.ReadObjectAsync<Settings>(_fileName);
            if (Settings == null)
            {
                Settings = new Settings();
                await SaveSettingsAsync();
                return false;
            }

            _loggingService.WriteInformation("Settings is loaded");
            return true;
        }

        public void MonitorSettingsChanges()
        {
            if (_watcher != null)
                return;

            _watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(_fileService.GetFilePath(_fileName)),
                EnableRaisingEvents = true,
                Filter = "*.*",
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.LastWrite
            };
            _watcher.Changed += _watcher_Changed;
        }

        public async Task UpdateSettingsAsync(User user, Channel channel = null)
        {
            Settings.User = user;
            if (channel != null)
            {
                _loggingService.WriteInformation($"Updated channel from {Settings.Channel?.Value} to {channel?.Value}");

                Settings.Channel = channel;
            }

            await SaveSettingsAsync();
        }

        public async Task UpdateSettingsAsync(int portNumber)
        {
            Settings.PortNumber = portNumber;
            await SaveSettingsAsync();

            _loggingService.WriteInformation($"Service port number {portNumber} is updated");
        }

        public async Task AddLocationAsync(Location location)
        {
            if (Settings.Locations.Any(l => l.Id == location.Id))
                return;

            Settings.Locations = Settings.Locations.Union(new[] { location }).ToList();
            await SaveSettingsAsync();

            _loggingService.WriteInformation($"Location {location.Name} is added");
        }

        public async Task RemoveLocationAsync(Location location)
        {
            var oldLocation = Settings.Locations.SingleOrDefault(l => l.Id == location.Id);
            if (oldLocation == null)
                return;

            Settings.Locations = Settings.Locations.Except(new[] { oldLocation }).ToList();
            await SaveSettingsAsync();

            _loggingService.WriteInformation($"Location {location.Name} is removed");
        }

        public async Task AddPrinterAsync(Printer printer)
        {
            if (Settings.Printers.Any(x => string.Compare(x.Name, printer.Name) == 0))
                return;

            var newPrinter = new Printer();
            newPrinter.Name = printer.Name;
            newPrinter.Register = printer.Register;
            newPrinter.Rotation = printer.Rotation;
            newPrinter.PrintMode = printer.PrintMode;

            var documentTypes = printer.DocumentTypes
                .Where(x => x.Enabled == true)
                .ToList();
            documentTypes.ForEach(x =>
            {
                if (x.Quantity <= 0)
                    x.Quantity = 1;
            });
            newPrinter.DocumentTypes = documentTypes;

            Settings.Printers = Settings.Printers.Union(new[] { newPrinter }).ToList();
            await SaveSettingsAsync();

            _loggingService.WriteInformation($"Printer {printer.Name} is added");
        }

        public async Task UpdatePrinterAsync(Printer printer)
        {
            var originalPrinter = Settings.Printers.SingleOrDefault(x => string.Compare(x.Name, printer.Name) == 0);
            if (originalPrinter == null)
                return;

            originalPrinter.Register = printer.Register;
            originalPrinter.Rotation = printer.Rotation;
            originalPrinter.PrintMode = printer.PrintMode;

            var documentTypes = printer.DocumentTypes
                .Where(x => x.Enabled == true)
                .ToList();
            documentTypes.ForEach(x =>
            {
                if (x.Quantity <= 0)
                    x.Quantity = 1;
            });
            originalPrinter.DocumentTypes = documentTypes;

            await SaveSettingsAsync();

            _loggingService.WriteInformation($"Printer {printer.Name} is updated");
        }

        public async Task RemovePrinterAsync(Printer printer)
        {
            var oldPrinter = Settings.Printers.SingleOrDefault(x => string.Compare(x.Name, printer.Name) == 0);
            if (oldPrinter == null)
                return;

            Settings.Printers = Settings.Printers.Except(new[] { oldPrinter }).ToList();
            await SaveSettingsAsync();

            _loggingService.WriteInformation($"Printer {printer.Name} is removed");
        }

        public async Task AddToStartup(bool startup)
        {
            bool result = false;

            await Task.Factory.StartNew(() =>
            {
                if (startup)
                    result = OnAddToStartup();
                else
                    result = OnRemoveFromStartup();
            });

            if (result)
            {
                Settings.AddedToStartup = startup;
                await SaveSettingsAsync();
            }
        }

        public async Task InstallService(bool install)
        {
            bool result = false;

            await Task.Factory.StartNew(() =>
            {
                if (install)
                    result = OnInstallService();
                else
                    result = OnUninstallService();
            });

            if (result)
            {
                Settings.InstalledService = install;
                await SaveSettingsAsync();
            }
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            lock (_locker)
            {
                var oldSettings = Settings;
                if (oldSettings == null)
                    return;

                var task = _fileService.ReadObjectAsync<Settings>(_fileName);
                task.Wait();
                Settings = task.Result;
                if (Settings == null)
                    return;

                if (oldSettings.Channel?.Value != Settings.Channel?.Value)
                    ChannelChangedEvent?.Invoke(Settings.Channel);

                if (oldSettings.PortNumber != Settings.PortNumber)
                    PortNumberChangedEvent?.Invoke(Settings.PortNumber);
            }
        }

        private async Task SaveSettingsAsync()
        {
            await _fileService.SaveObjectAsync(_fileName, Settings);
        }

        private string GetAppLocation()
        {
            return System.Reflection.Assembly.GetEntryAssembly().Location;
        }

        private bool OnAddToStartup()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C REG ADD \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\" /V \"{ Process.GetCurrentProcess().ProcessName }\" /T REG_SZ /F /D \"{ GetAppLocation() }\"",
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = Process.Start(psi);
                process.WaitForExit();

                _loggingService.WriteInformation($"An application is added to Startup");

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.WriteWarning($"An application is not added to Startup");

                Debug.WriteLine($"Error in {nameof(SettingsService)}: {ex.ToString()}");
                _loggingService.WriteError(ex);

                return false;
            }
        }

        private bool OnRemoveFromStartup()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C REG DELETE \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\" /V \"{ Process.GetCurrentProcess().ProcessName }\" /F",
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = Process.Start(psi);
                process.WaitForExit();

                _loggingService.WriteInformation($"An application is removed from Startup");

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.WriteWarning($"An application is not removed from Startup");

                Debug.WriteLine($"Error in {nameof(SettingsService)}: {ex.ToString()}");
                _loggingService.WriteError(ex);

                return false;
            }
        }

        private string GetServiceLocation()
        {
            var appLocation = GetAppLocation();
            var serviceLocation = $"{ Path.GetDirectoryName(appLocation) }\\AutoPrintr.Service.exe";
            return serviceLocation;
        }

        private bool OnInstallService()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = GetServiceLocation(),
                    Arguments = "/Stop /Uninstall /Install /Start",
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };

                var process = Process.Start(psi);
                process.WaitForExit();

                _loggingService.WriteInformation($"Service is installed");

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.WriteWarning($"Service is not installed");

                Debug.WriteLine($"Error in {nameof(SettingsService)}: {ex.ToString()}");
                _loggingService.WriteError(ex);

                return false;
            }
        }

        private bool OnUninstallService()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = GetServiceLocation(),
                    Arguments = "/Stop /Uninstall",
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };

                var process = Process.Start(psi);
                process.WaitForExit();

                _loggingService.WriteInformation($"Service is uninstalled");

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.WriteWarning($"Service is not uninstalled");

                Debug.WriteLine($"Error in {nameof(SettingsService)}: {ex.ToString()}");
                _loggingService.WriteError(ex);

                return false;
            }
        }
        #endregion
    }
}