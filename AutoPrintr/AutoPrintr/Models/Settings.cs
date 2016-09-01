using System.Collections.Generic;

namespace AutoPrintr.Models
{
    public class Settings
    {
        public Channel Channel { get; set; }
        public User User { get; set; }
        public IEnumerable<Printer> Printers { get; set; }
    }
}