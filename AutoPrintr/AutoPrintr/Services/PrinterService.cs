using AutoPrintr.IServices;
using AutoPrintr.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<IEnumerable<Printer>> GetPrintersAsync()
        {
            return await Task.Factory.StartNew<IEnumerable<Printer>>(() =>
            {
                var printers = GetInstalledPrinters();

                var result = new List<Printer>();
                foreach (var printer in printers)
                {
                    var existing = _settingsService.Settings.Printers.SingleOrDefault(x => string.Compare(x.Name, printer.Name, true) == 0);
                    result.Add(existing ?? printer);
                }

                return result;
            });
        }

        public async Task<bool> PrintDocumentAsync(Document document)
        {
            //TODO: Print document. Just read file and print it
            throw new NotImplementedException();
        }

        private IEnumerable<Printer> GetInstalledPrinters()
        {
            foreach (string item in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                yield return new Printer { Name = item };
        }
        #endregion
    }
}