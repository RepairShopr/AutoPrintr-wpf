using System;

namespace AutoPrintr.Models
{
    public class Job : BaseModel
    {
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public JobState State { get; set; }
        public Document Document { get; set; }
        public Printer Printer { get; set; }
        public double DownloadProgress { get; set; }
        public Exception Error { get; set; }

        public Job()
        {
            CreatedOn = DateTime.Now;
            State = JobState.New;
        }
    }
}