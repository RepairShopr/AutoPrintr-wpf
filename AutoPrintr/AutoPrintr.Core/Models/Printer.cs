using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace AutoPrintr.Core.Models
{
    [DataContract]
    public class Printer : BaseModel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int? Register { get; set; }
        [DataMember]
        public bool Rotation { get; set; }
        [DataMember]
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