#nullable enable

namespace BVA200.Models
{
    public class DetectorSettings
    {
        public int AcqMode { get; set; }
        public string Detector { get; set; } //"Topaz"
        public decimal CountTime { get; set; }
        public int CountBGTime { get; set; }
        public int SampleVol { get; set; }
        public decimal TracerDisseminationTime { get; set; }
        public int FineGain { get; set; }
        public int CoarseGain { get; set; }
        public int LLD { get; set; }
        public int CountLimit { get; set; }
    }
}