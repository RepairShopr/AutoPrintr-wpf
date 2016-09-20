using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPrintr.Core.Services
{
    internal class PrinterService : IPrinterService
    {
        #region Properties
        private readonly ISettingsService _settingsService;
        private readonly IFileService _fileService;
        #endregion

        #region Constructors
        public PrinterService(ISettingsService settingsService,
            IFileService fileService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
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
                    var existingCopy = existing?.Clone() as Printer;
                    result.Add(existingCopy ?? printer);
                }

                return result;
            });
        }

        public async Task PrintDocumentAsync(Printer printer, Document document, Action<bool, Exception> completed = null)
        {
            await Task.Factory.StartNew(() =>
            {
                Exception exception = null;
                try
                {
                    if (string.IsNullOrEmpty(document.LocalFilePath))
                        throw new InvalidOperationException("LocalFilePath is required");

                    var processPath = ExtractSumatraPDF();
                    var printProcess = Process.Start(processPath, $"-silent -exit-on-print -print-to \"{printer.Name}\" \"{_fileService.GetFilePath(document.LocalFilePath)}\"");
                    printProcess.WaitForExit();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    completed?.Invoke(exception == null, exception);
                }
            });
        }

        private IEnumerable<Printer> GetInstalledPrinters()
        {
            foreach (string item in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                yield return new Printer { Name = item };
        }

        private string ExtractSumatraPDF()
        {
            string path = Path.Combine(Path.GetTempPath(), $"{nameof(Resources.SumatraPDF)}.exe");

            if (!File.Exists(path))
                File.WriteAllBytes(path, Resources.SumatraPDF);

            return path;
        }
        #endregion
    }
}