using System;
using System.Collections.Generic;

namespace BVA200.Models
{
    public class CalibrationLog
    {
        public Int32 ID { get; set; }
        public DateTime Timestamp { get; set; }
        public string Standard{get;set;}
        public string StoredPeaks { get; set; }
    }
}