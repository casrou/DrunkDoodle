using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrunkDoodle.Server.Models
{
    public class Player
    {
        public int team { get; set; }
        public string name { get; set; }
        public int score { get; set; }
    }
}
