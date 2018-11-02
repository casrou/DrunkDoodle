using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrunkDoodle.Server.Models
{
    public class Room
    {
        public int roomId { get; set; }
        public string artistDevice { get; set; }
        public List<string> audienceDevices { get; set; } = new List<string>();

        public Queue<Player> players { get; set; }
        public Drawing currentDrawing { get; set; }
        public RoomRules roomRules { get; set; }
    }

    public class RoomRules
    {
        public int drinkAmount { get; set; }
        public string drinkType { get; set; }
        public string wordLanguage { get; set; }
    }
}
