using System;

namespace AutoPrintr.Models
{
    public class Job
    {
        public int Location { get; set; }
        public JobState State { get; set; }
        public Document Document { get; set; }
        public double DownloadProgress { get; set; }
        public Exception Error { get; set; }
    }
}