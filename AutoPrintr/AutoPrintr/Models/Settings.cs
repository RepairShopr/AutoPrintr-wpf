using System.Collections.Generic;

namespace AutoPrintr.Models
{
    public class Settings
    {
        public string Channel { get; set; }
        public IEnumerable<Location> Locations { get; set; }
        public IEnumerable<Printer> Printers { get; set; }
    }
}