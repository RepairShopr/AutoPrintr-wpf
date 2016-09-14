using System;

namespace AutoPrintr.Core.Models
{
    public class Job : BaseModel
    {
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public JobState State { get; set; }
        public Document Document { get; set; }
        public string Printer { get; set; }
        public int Quantity { get; set; }
        public double DownloadProgress { get; set; }
        public Exception Error { get; set; }

        public Job()
        {
            CreatedOn = DateTime.Now;
            State = JobState.New;
        }
    }
}