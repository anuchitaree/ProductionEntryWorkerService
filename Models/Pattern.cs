using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionEntryWorkerService.Models
{
    public class Pattern
    {
        public int TotalLength { get; set; }

        public int Start1 { get; set; }
        public int Length1 { get; set; }
        public int Start2 { get; set; }
        public int Length2 { get; set; }

    }
}
