using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCinelMaui.Models.Dtos
{
    public class BoughtTicketDto
    {
        public int Id { get; set; }
        public string SeatNumber { get; set; }
        public FlightDto Flight { get; set; }
    }
}
