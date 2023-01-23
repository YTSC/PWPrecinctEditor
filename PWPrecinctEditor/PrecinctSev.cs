using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWPrecinctEditor
{
    class PrecinctSev
    {
        public string Path { get; set; }
        public uint Version { get; set; }
        public uint Key { get; set; }
        public List<AreaSev> Areas = new List<AreaSev>();

                
    }
}
