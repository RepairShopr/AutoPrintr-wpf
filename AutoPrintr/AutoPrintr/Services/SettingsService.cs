using AutoPrintr.IServices;
using AutoPrintr.Models;
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
        }

        public async Task SaveSettingsAsync()
        {
            await _fileService.SaveFileAsync(nameof(Settings), Settings);
        }
        #endregion
    }
}