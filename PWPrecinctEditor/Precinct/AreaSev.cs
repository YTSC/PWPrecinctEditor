using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWPrecinctEditor
{
    class AreaSev
    {
        public int PointsCount { get; set; }
        public int DomainID { get; set; }
        public int Priority { get; set; }
        public int OriginMapID { get; set; }
        public int DestinationMapID { get; set; }
        public bool PKProtection { get; set; }
        public Point Respawn { get; set; }
        public List<Point> Points = new List<Point>();
    }
}
