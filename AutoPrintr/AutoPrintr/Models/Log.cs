using System;

namespace AutoPrintr.Models
{
    public class Log
    {
        public DateTime DateTime { get; set; }
        public string Event { get; set; }
        public LogType Type { get; set; }
    }
}