using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWPrecinctEditor
{
    class Point
    {
        public int ID { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public Point()
        {

        }
        public Point(int ID, float X, float Y, float Z)
        {
            this.ID = ID;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        
    }
}
