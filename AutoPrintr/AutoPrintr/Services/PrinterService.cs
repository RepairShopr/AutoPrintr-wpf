using AutoPrintr.IServices;
using AutoPrintr.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoPrintr.Services
{
    internal class PrinterService : IPrinterService
    {
        #region Properties
        private readonly ISettingsService _settingsService;
        #endregion

        #region Constructors
        public PrinterService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }
        #endregion

        #region Methods
        public IEnumerable<Printer> GetInstalledPrinters()
        {
            foreach (string item in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                yield return new Printer { Name = item };
        }

        public IEnumerable<Printer> GetPrintersFromSettings()
        {
            return _settingsService.Settings.Printers;
        }

        public async Task<bool> PrintDocumentAsync(Document document)
        {
            //TODO: Print document. Just read file and print it
            throw new NotImplementedException();
        }
        #endregion
    }
}