namespace BVA200.Models
{
    public class Peak
    {
        public int Start { get; set; }
        public int End { get; set; }
        public double Centroid { get; set; }
        public double Significance { get; set; }
        public double ExpectedFullWidthHalfMax { get; set; }
        public double Energy { get; set; }
        public uint Counts { get; set; }
    }
}