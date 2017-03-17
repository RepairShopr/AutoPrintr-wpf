using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace AutoPrintr.Core.Models
{
    [DataContract]
    public class Job : BaseModel
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public DateTime CreatedOn { get; set; }
        [DataMember]
        public DateTime? UpdatedOn { get; set; }
        [DataMember]
        public JobState State { get; set; }
        [DataMember]
        public Document Document { get; set; }
        [DataMember]
        public string Printer { get; set; }
        [DataMember]
        public int Quantity { get; set; }
        [DataMember]
        public double DownloadProgress { get; set; }
        [JsonProperty]
        public Exception Error { get; set; }

        public Job()
        {
            Id = Guid.NewGuid();
            CreatedOn = DateTime.Now;
            State = JobState.New;
        }
    }
}