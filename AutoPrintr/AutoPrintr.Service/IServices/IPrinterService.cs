using AutoPrintr.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoPrintr.Service.IServices
{
    public interface IPrinterService
    {
        Task<IEnumerable<Printer>> GetPrintersAsync();
        Task PrintDocumentAsync(Printer printer, Document document, int count, Action<bool, Exception> completed = null);
    }
}