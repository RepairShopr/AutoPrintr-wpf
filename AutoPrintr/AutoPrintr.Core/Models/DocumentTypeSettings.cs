namespace AutoPrintr.Core.Models
{
    public class DocumentTypeSettings : BaseModel
    {
        public bool Enabled { get; set; }
        public int Quantity { get; set; }
        public bool AutoPrint { get; set; }
        public DocumentType DocumentType { get; set; }
    }
}