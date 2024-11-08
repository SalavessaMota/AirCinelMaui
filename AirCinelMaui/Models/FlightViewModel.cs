using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCinelMaui.Models
{
    public class FlightViewModel
    {
        public string FlightNumber { get; set; }
        public string FlightDuration { get; set; }
        public string DepartureAirport { get; set; }
        public string ArrivalAirport { get; set; }
        public string DepartureAirportImage { get; set; }
        public string ArrivalAirportImage { get; set; }
        public string AirplaneImage { get; set; }
        public string AirplaneInfo { get; set; }
    }
}
