using AutoPrintr.Core.Helpers;
using Newtonsoft.Json;
using System;

namespace AutoPrintr.Core.Models
{
    public class Document : BaseModel
    {
        [JsonIgnore]
        public string TypeTitle => GetTypeTitle(Type);

        [JsonIgnore]
        public string SizeTitle => GetSizeTitle(Size);

        public string LocalFilePath { get; set; }

        [JsonProperty("file")]
        public Uri FileUri { get; set; }

        [JsonProperty("autoprinted")]
        public bool AutoPrint { get; set; }

        [JsonProperty("document")]
        public DocumentType Type { get; set; }

        [JsonProperty("location")]
        public int? Location { get; set; }

        [JsonProperty("register")]
        public int? Register { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(DocumentSizeJsonConverter))]
        public DocumentSize Size { get; set; }

        public static string GetTypeTitle(DocumentType type)
        {
            switch (type)
            {
                case DocumentType.Invoice: return "Invoice";
                case DocumentType.Estimate: return "Estimate";
                case DocumentType.Ticket: return "Ticket";
                case DocumentType.IntakeForm: return "Intake Form";
                case DocumentType.OuttakeForm: return "Outtake Form";
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

        public static string GetSizeTitle(DocumentSize type)
        {
            switch (type)
            {
                case DocumentSize.Letter: return "Size: Letter";
                case DocumentSize.Label: return "Size: 1.1x3";
                case DocumentSize.Receipt: return "Size: 80mm";
                case DocumentSize.IntakeForm: return "Intake Form";
                case DocumentSize.OuttakeForm: return "Outtake Form";
                case DocumentSize.PopDrawer: return "Pop Drawer";
                default: return null;
            }
        }
    }
}