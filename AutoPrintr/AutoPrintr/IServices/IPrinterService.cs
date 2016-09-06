﻿using AutoPrintr.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoPrintr.IServices
{
    public interface IPrinterService
    {
        Task<IEnumerable<Printer>> GetPrintersAsync();
        Task PrintDocumentAsync(Printer printer, Document document, Action<bool, Exception> completed = null);
    }
}