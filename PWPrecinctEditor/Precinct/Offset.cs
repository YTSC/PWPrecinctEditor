using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWPrecinctEditor
{
    public class Offset
    {
        public string name { get; }
        public string baseOffset { get; }
        public string posX { get;}
        public string posY { get; }
        public string posZ { get; }

        public Offset(string name, string baseOffset, string posX, string posY, string posZ)
        {
            this.name = name;
            this.baseOffset = baseOffset;
            this.posX = posX;
            this.posY = posY;
            this.posZ = posZ;
        }
    }
}

