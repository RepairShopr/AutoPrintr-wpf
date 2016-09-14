using System.Collections.Generic;

namespace AutoPrintr.Core.Models
{
    public class Settings : BaseModel
    {
        public Channel Channel { get; set; }
        public User User { get; set; }
        public IEnumerable<Printer> Printers { get; set; }
        public IEnumerable<Location> Locations { get; set; }

        public Settings()
        {
            Printers = new List<Printer>();
            Locations = new List<Location>();
        }
    }
}