﻿using AutoPrintr.IServices;
using AutoPrintr.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPrintr.Services
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

                    var printProcess = Process.Start("SumatraPDF.exe", $"-silent -exit-on-print -print-to \"{printer.Name}\" \"{_fileService.GetFilePath(document.LocalFilePath)}\"");
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
        #endregion
    }
}