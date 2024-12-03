using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCinelMaui.Models.Dtos
{
    public class TicketDto
    {
        public int Id { get; set; }
        public string SeatNumber { get; set; }
        public UserDto User { get; set; }
    }
}
