using System.Collections.Generic;

namespace AutoPrintr.Models
{
    public class Printer
    {
        public string Name { get; set; }
        public IEnumerable<DocumentTypeSettings> DocumentTypes { get; set; }

        public Printer()
        {
            DocumentTypes = new List<DocumentTypeSettings>();
        }
    }
}