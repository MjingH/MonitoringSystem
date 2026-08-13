using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringSystem.Model
{
    public class StorageModel
    {
        public int id { get; set; }
        public String SlaveAddress { get; set; }
        public String FuncCode { get; set; }

        public String StartAddress { get; set; }
        public int Length { get; set; }
    }
}
    