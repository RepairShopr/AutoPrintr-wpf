using System;

namespace AutoPrintr.Models
{
    public class Document
    {
        public string Name { get; set; }
        public string Title => GetTitle(Type);
        public string LocalFilePath { get; set; }
        public Uri FileUri { get; set; }
        public DocumentType Type { get; set; }

        public static string GetTitle(DocumentType type)
        {
            switch (type)
            {
                case DocumentType.Invoice: return "Invoice";
                case DocumentType.Estimate: return "Estimate";
                case DocumentType.Ticket: return "Ticket";
                case DocumentType.IntakeForm: return "Intake Form";
                case DocumentType.Receipt: return "Receipt";
                case DocumentType.ZReport: return "Z Report";
                case DocumentType.TicketReceipt: return "Ticket Receipt";
                case DocumentType.PopDrawer: return "Pop Drawer";
                case DocumentType.Adjustment: return "Adjustment";
                case DocumentType.CustomerID: return "Customer ID";
                case DocumentType.Asset: return "Asset";
                case DocumentType.TicketLabel: return "Ticket Label";
                default: return null;
            }
        }
    }
}