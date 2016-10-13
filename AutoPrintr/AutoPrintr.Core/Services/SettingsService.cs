using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPrintr.Core.Services
{
    internal class SettingsService : ISettingsService
    {
        #region Properties
        private readonly IFileService _fileService;
        private readonly ILoggerService _loggingService;
        private readonly string _fileName = $"{nameof(Settings)}.json";

        public event ChannelChangedEventHandler ChannelChangedEvent;

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
        public async Task LoadSettingsAsync()
        {
            _loggingService.WriteInformation("Starting load settings");

            Settings = await _fileService.ReadObjectAsync<Settings>(_fileName);
            if (Settings == null)
            {
                Settings = new Settings();
                if (Settings.AddToStartup)
                    OnAddToStartup();
            }

            _loggingService.WriteInformation("Settings is loaded");
        }

        public async Task SetSettingsAsync(User user, Channel channel = null)
        {
            Settings.User = user;
            if (channel != null)
            {
                _loggingService.WriteInformation($"Updated channel from {Settings.Channel} to {channel}");

                Settings.Channel = channel;
                ChannelChangedEvent?.Invoke(Settings.Channel);
            }

            await SaveSettingsAsync();
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
            newPrinter.DocumentTypes = printer.DocumentTypes.Where(x => x.Enabled == true).ToList();

            Settings.Printers = Settings.Printers.Union(new[] { newPrinter }).ToList();
            await SaveSettingsAsync();

            _loggingService.WriteInformation($"Printer {printer.Name} is added");
        }

        public async Task UpdatePrinterAsync(Printer printer)
        {
            var originalPrinter = Settings.Printers.SingleOrDefault(x => string.Compare(x.Name, printer.Name) == 0);
            if (originalPrinter == null)
                return;

            originalPrinter.DocumentTypes = printer.DocumentTypes.Where(x => x.Enabled == true).ToList();
            originalPrinter.Register = printer.Register;
            originalPrinter.Rotation = printer.Rotation;
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

        public async void AddToStartup(bool startup)
        {
            Settings.AddToStartup = startup;
            await SaveSettingsAsync();

            if (Settings.AddToStartup)
                OnAddToStartup();
            else
                OnRemoveFromStartup();
        }

        private async Task SaveSettingsAsync()
        {
            await _fileService.SaveObjectAsync(_fileName, Settings);
        }

        private void OnAddToStartup()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C REG ADD \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\" /V \"{ Process.GetCurrentProcess().ProcessName}\" /T REG_SZ /F /D \"{System.Reflection.Assembly.GetEntryAssembly().Location}\"",
                Verb = "runas",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var process = Process.Start(psi);
            process.WaitForExit();

            _loggingService.WriteInformation($"An application is added to Startup");
        }

        private void OnRemoveFromStartup()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C REG DELETE \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\" /V \"{ Process.GetCurrentProcess().ProcessName}\" /F",
                Verb = "runas",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var process = Process.Start(psi);
            process.WaitForExit();

            _loggingService.WriteInformation($"An application is removed from Startup");
        }
        #endregion
    }
}