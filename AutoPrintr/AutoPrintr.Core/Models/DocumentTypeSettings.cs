using System.Runtime.Serialization;

namespace AutoPrintr.Core.Models
{
    [DataContract]
    public class DocumentTypeSettings : BaseModel
    {
        [DataMember]
        public bool Enabled { get; set; }
        [DataMember]
        public int Quantity { get; set; }
        [DataMember]
        public bool AutoPrint { get; set; }
        [DataMember]
        public DocumentType DocumentType { get; set; }
    }
}