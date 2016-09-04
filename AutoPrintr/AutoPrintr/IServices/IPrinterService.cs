using AutoPrintr.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoPrintr.IServices
{
    public interface IPrinterService
    {
        Task<IEnumerable<Printer>> GetPrintersAsync();
        Task<bool> PrintDocumentAsync(Document document);
    }
}