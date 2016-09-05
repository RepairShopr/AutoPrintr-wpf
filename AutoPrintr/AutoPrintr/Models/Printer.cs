using System.Collections.Generic;
using System.Linq;

namespace AutoPrintr.Models
{
    public class Printer : BaseModel
    {
        public string Name { get; set; }
        public IEnumerable<DocumentTypeSettings> DocumentTypes { get; set; }

        public Printer()
        {
            DocumentTypes = new List<DocumentTypeSettings>();
        }

        public override object Clone()
        {
            var newObj = (Printer)base.Clone();
            newObj.DocumentTypes = DocumentTypes.Select(x => (DocumentTypeSettings)x.Clone()).ToList();
            return newObj;
        }
    }
}