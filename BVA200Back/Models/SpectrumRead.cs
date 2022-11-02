using System;
using System.Collections.Generic;
using System.Linq;

namespace BVA200.Models
{
    public class SpectrumRead : DetectorSettings
    {
        public float LiveTime { get; set; }
        public float RealTime { get; set; }
        public string LiveTimeString { get; set; }
        public string RealTimeString { get; set; }
        public float HighVoltage { get; set; }
        public int HighVoltageStatus { get; set; }
        public int DigitalGain { get; set; }
        public decimal CurrentCounts { get; set; }
        public List<uint> DetectorSpectrum { get; set; }
        public IEnumerable<Peak> Peaks { get; set; }
        public string Result { get; set; }
        public int LowChannel { get; set; }
        public int HighChannel { get; set; }
        public Sample CalculatedSample { get; set; }

        public void Calculate(int? mode = 1)
        {
            /*
                Modes:
                    0:  Means we're rolling with the high/low channel that 
                        the Peak Search algorithm came up with.

                    1:  We are going to find the centroid and add capture 55 or 56
                        in either direction.

                    2:  Statically set the low/high channels to 308 & 420 respectively.

            */
            
            UInt32[] tSpectrum = this.DetectorSpectrum.ToArray();

            double highestPeakSignificance = 0;
            int highestPeakIndex = -1;
            int index = 0;
            foreach(var peak in this.Peaks){
                if(peak.Centroid < 200){
                    peak.Significance = peak.Significance * -1;
                }
            }
            foreach (var peak in this.Peaks)
            {
                int centroid = (int)peak.Centroid;
                switch (mode){
                    case 0:
                        break;
                    case 1:
                        peak.Start = centroid - 55;
                        peak.End = centroid + 56;
                        break;
                    case 2:
                        peak.Start = 308;
                        peak.End = 420;
                        break;
                    default:
                        break;
                }

                var roi = tSpectrum.Skip(peak.Start).Take(peak.End - peak.Start);
                uint counts = roi.Aggregate((x, y) => x + y);
                peak.Counts = counts;

                
                var significance = peak.Significance;
                if (significance > highestPeakSignificance)
                {
                    highestPeakSignificance = significance;
                    highestPeakIndex = index;
                }

                index++;
            }

            if (this.Peaks.Count() < 1)
            {
                this.Peaks = null;
                this.CalculatedSample = null;
            }
            else
            {
                string sampleName = "Unknown";
                Peak tPeak = this.Peaks.ToArray()[highestPeakIndex];
                int centroid = (int)tPeak.Centroid;
                if (centroid >= 300 && centroid <= 500)
                {
                    sampleName = "BVA Tracer";
                }
                else if (centroid >= 501 && centroid <= 900)
                {
                    sampleName = "Cesium";
                }

                if (sampleName != "Unknown"){
                    this.CalculatedSample = new Sample{
                        Centroid = centroid,
                        HighChannel = tPeak.End,
                        LowChannel = tPeak.Start,
                        SampleName = sampleName,
                        Counts = (int)tPeak.Counts
                    };
                }
            }
        }
    }
}
