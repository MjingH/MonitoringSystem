using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringSystem.Base
{
    public enum MonitorValueState
    {
        OK = 0,
        LoLo =1,
        Low = 2,
        High = 3,
        HiHi = 4,
    }
}
