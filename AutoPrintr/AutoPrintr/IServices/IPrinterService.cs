using AutoPrintr.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoPrintr.IServices
{
    public interface IPrinterService
    {
        IEnumerable<Printer> GetInstalledPrinters();
        IEnumerable<Printer> GetPrintersFromSettings();
        Task<bool> PrintDocumentAsync(Document document);
    }
}