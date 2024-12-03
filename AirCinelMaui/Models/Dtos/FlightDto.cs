using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCinelMaui.Models.Dtos
{
    public class FlightDto
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public AirplaneDto Airplane { get; set; }
        public AirportDto DepartureAirport { get; set; }
        public AirportDto ArrivalAirport { get; set; }
        public List<TicketDto> Tickets { get; set; }
    }
}
