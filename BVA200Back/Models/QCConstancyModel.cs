using System;

namespace BVA200.Models
{
    public class QCModel : DetectorSettings
    {
        public DateTime? Timestamp { get; set; }
        public int? CPM { get; set; }
        public int LowChannel { get; set; }
        public int HighChannel { get; set; }
    }
}