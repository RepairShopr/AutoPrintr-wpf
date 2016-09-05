using AutoPrintr.IServices;
using AutoPrintr.Models;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPrintr.Services
{
    internal class SettingsService : ISettingsService
    {
        #region Properties
        private readonly IFileService _fileService;

        public Settings Settings { get; private set; }
        #endregion

        #region Constructors
        public SettingsService(IFileService fileService)
        {
            _fileService = fileService;
        }
        #endregion

        #region Methods
        public async Task LoadSettingsAsync()
        {
            Settings = await _fileService.ReadObjectAsync<Settings>(nameof(Settings));
            if (Settings == null)
                Settings = new Settings();
        }

        public async Task SetSettingsAsync(User user, Channel channel = null)
        {
            Settings.User = user;
            if (channel != null)
                Settings.Channel = channel;

            await SaveSettingsAsync();
        }

        public async Task AddLocationAsync(Location location)
        {
            if (Settings.Locations.Any(l => l.Id == location.Id))
                return;

            Settings.Locations = Settings.Locations.Union(new[] { location }).ToList();
            await SaveSettingsAsync();
        }

        public async Task RemoveLocationAsync(Location location)
        {
            var oldLocation = Settings.Locations.SingleOrDefault(l => l.Id == location.Id);
            if (oldLocation == null)
                return;

            Settings.Locations = Settings.Locations.Except(new[] { oldLocation }).ToList();
            await SaveSettingsAsync();
        }

        public async Task AddPrinterAsync(Printer printer)
        {
            if (Settings.Printers.Any(x => string.Compare(x.Name, printer.Name) == 0))
                return;

            var newPrinter = new Printer();
            newPrinter.Name = printer.Name;
            newPrinter.DocumentTypes = printer.DocumentTypes.Where(x => x.Enabled == true).ToList();

            Settings.Printers = Settings.Printers.Union(new[] { newPrinter }).ToList();
            await SaveSettingsAsync();
        }

        public async Task UpdatePrinterAsync(Printer printer)
        {
            var originalPrinter = Settings.Printers.SingleOrDefault(x => string.Compare(x.Name, printer.Name) == 0);
            if (originalPrinter == null)
                return;

            originalPrinter.DocumentTypes = printer.DocumentTypes.Where(x => x.Enabled == true).ToList();
            await SaveSettingsAsync();
        }

        public async Task RemovePrinterAsync(Printer printer)
        {
            var oldPrinter = Settings.Printers.SingleOrDefault(x => string.Compare(x.Name, printer.Name) == 0);
            if (oldPrinter == null)
                return;

            Settings.Printers = Settings.Printers.Except(new[] { oldPrinter }).ToList();
            await SaveSettingsAsync();
        }

        private async Task SaveSettingsAsync()
        {
            await _fileService.SaveObjectAsync(nameof(Settings), Settings);
        }
        #endregion
    }
}