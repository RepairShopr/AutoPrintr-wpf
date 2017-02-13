using System;
using System.Collections.Generic;

namespace AutoPrintr.Core.Models
{
    public class Settings : BaseModel
    {
        private const int _defaultPortNumber = 7775;
        private int _portNumber;

        public bool AddedToStartup { get; set; }
        public bool InstalledService { get; set; }
        public int PortNumber
        {
            get { return Math.Max(_defaultPortNumber, _portNumber); }
            set { _portNumber = value; }
        }
        public Channel Channel { get; set; }
        public User User { get; set; }
        public IEnumerable<Printer> Printers { get; set; }
        public IEnumerable<Location> Locations { get; set; }

        public Settings()
        {
            PortNumber = _defaultPortNumber;
            Printers = new List<Printer>();
            Locations = new List<Location>();
        }
    }
}