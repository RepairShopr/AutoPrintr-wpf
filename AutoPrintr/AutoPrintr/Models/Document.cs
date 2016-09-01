using System;

namespace AutoPrintr.Models
{
    public class Document
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string LocalFilePath { get; set; }
        public Uri FileUri { get; set; }
        public DocumentType Type { get; set; }
    }
}