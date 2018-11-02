using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrunkDoodle.Server.Models
{
    public class Drawing
    {
        public Word word { get; set; }
        public Player artist { get; set; }
        public IList<DrawPoint> drawPoints { get; set; }
    }

    public class DrawPoint
    {
        public double x { get; set; }
        public double y { get; set; }
        public bool dragging { get; set; }
    }
}
