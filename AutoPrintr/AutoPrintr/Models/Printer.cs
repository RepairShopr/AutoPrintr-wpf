using System.Collections.Generic;

namespace AutoPrintr.Models
{
    public class Printer
    {
        public string Name { get; set; }
        public IEnumerable<DocumentType> DocumentTypes { get; set; }
    }
}