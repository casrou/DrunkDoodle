using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrunkDoodle.Server.Models
{
    public class Team
    {
        public int teamNo { get; set; }
        public List<Player> players { get; set; } = new List<Player>();
        public int teamScore { get; set; }
    }
}
