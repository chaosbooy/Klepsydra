using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klepsydra.Resources.Scripts
{
    public struct Data
    {
        public int lastTime { get; set; }
        public Dictionary<string, int> timers { get; set; }

        public Data()
        {
            lastTime = 10;
            timers = new Dictionary<string, int>();
        }
    }
}
