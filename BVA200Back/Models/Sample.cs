using System.Collections.Generic;

namespace BVA200.Models
{
    public class Sample
    {
        public int LowChannel { get; set; }
        public int HighChannel { get; set; }
        public int Centroid { get; set; }
        public int Counts { get; set; }
        public string SampleName { get; set; }
    }
}