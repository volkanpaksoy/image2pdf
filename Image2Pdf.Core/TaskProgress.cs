using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image2Pdf.Core
{
    public class TaskProgress
    {
        public string StatusMessage { get; set; }
        public double CompletedPercentage { get; set; }
        public int ProcessedInputCount { get; set; }
    }
}
