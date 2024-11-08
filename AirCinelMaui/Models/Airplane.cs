using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCinelMaui.Models
{
    public class Airplane
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public int Capacity { get; set; }
        public string ImageFullPath { get; set; }
    }
}
