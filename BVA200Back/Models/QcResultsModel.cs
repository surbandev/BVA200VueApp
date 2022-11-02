using System;

namespace BVA200.Models
{
    public class QcResultsModel
    {
        public bool EfficiencyStatusPassed { get; set; }
        public bool EfficiencyAgeAcceptable { get; set; }
        public bool ConstancyStatusPassed { get; set; }
        public bool ConstancyAgeAcceptable { get; set; }
        public bool LinearityStatusPassed { get; set; }
        public bool LinearityAgeAcceptable { get; set; }
        public bool RecentCalibrationPassed { get; set; }
    }
}