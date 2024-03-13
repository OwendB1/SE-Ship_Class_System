using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedVsBlueClassSystem
{
    public static class Settings
    {
        public static readonly int MAX_GRID_PROCESSED_PER_TICK = 5;
        public static readonly bool Debug = true;
        public static readonly int LOG_LEVEL = 2;//messages with logPriority >= this will get logged, less than will be ignored
        public static readonly int CLIENT_OUTPUT_LOG_LEVEL = 3;//messages with logPriority >= this will get output to clients
        public static readonly ushort COMMS_MESSAGE_ID = 53642;
    }
}
