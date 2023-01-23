using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWPrecinctEditor
{
    class PrecinctClt
    {
        public string Path { get; set; }
        public string Map { get; set; }
        public int Version { get; set; }
        public int Key { get; set; }
        public List<Area> Areas = new List<Area>();
        public bool hasChanged { get; set; } = false;

        public List<Image> worldMap = new List<Image>();
    }
}
