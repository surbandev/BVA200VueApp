#nullable enable
//https://stackoverflow.com/questions/55492214/the-annotation-for-nullable-reference-types-should-only-be-used-in-code-within-a
using System;
using Newtonsoft.Json;
namespace BVA200.Models
{
    public class TestResult
    {
        private int[] backgroundSpectrum;
        private int[] baselineSpectrum;
        private int[] standardSpectrum;
        private int[] postInjection1Spectrum;
        private int[] postInjection2Spectrum;
        private int[] postInjection3Spectrum;
        private int[] postInjection4Spectrum;
        private int[] postInjection5Spectrum;

        private int backgroundCount = 0;
        private int baselineCount = 0;
        private int standardCount = 0;
        private int postInjection1Count = 0;
        private int postInjection2Count = 0;
        private int postInjection3Count = 0;
        private int postInjection4Count = 0;
        private int postInjection5Count = 0;

        public int? ID { get; set; }
        public string PatientID { get; set; }
        public decimal Weight { get; set; }
        public decimal Height { get; set; }
        public int Age { get; set; }
        public string? Sex { get; set; }
        public string OrderingPhysician { get; set; }
        public int Amputee { get; set; }
        public int AmputeeCorrectionPercentage { get; set; }
        public bool Pregnant { get; set; }
        public string InjectateLotNumber { get; set; }
        public Decimal StandardStrength { get; set; }
        public DateTime InjectionTimestamp { get; set; }
        public int BackgroundCountMins { get; set; }
        public int TracerDisMins { get; set; }
        public int SampleCountMins { get; set; }
        public int LowChannel { get; set; }
        public int HighChannel { get; set; }
        public Object? BackgroundSpectrum
        {
            get
            {
                return backgroundSpectrum;
            }
            set
            {
                if (value is null)
                {
                    backgroundSpectrum = new int[] { };
                    return;
                }
                Type t = value.GetType();
                if (t == typeof(System.Int32[]))
                {
                    backgroundSpectrum = (int[])value;
                }
                else
                {
                    backgroundSpectrum = JsonConvert.DeserializeObject<int[]>(value.ToString());
                }
            }
        }
        public int BackgroundCount
        {
            get
            {
                return backgroundCount;
            }
            set
            {
                backgroundCount = value;
            }
        }
        public Object? BaselineSpectrum
        {
            get
            {
                return baselineSpectrum;
            }
            set
            {
                if (value is null)
                {
                    baselineSpectrum = new int[] { };
                    return;
                }
                Type t = value.GetType();
                if (t == typeof(System.Int32[]))
                {
                    baselineSpectrum = (int[])value;
                }
                else
                {
                    baselineSpectrum = JsonConvert.DeserializeObject<int[]>(value.ToString());
                }
            }
        }
        public int BaselineCount
        {
            get
            {
                return baselineCount;
            }
            set
            {
                baselineCount = value;
            }
        }
        public Object? StandardSpectrum
        {
            get
            {
                return standardSpectrum;
            }
            set
            {
                if (value is null)
                {
                    standardSpectrum = new int[] { };
                    return;
                }
                Type t = value.GetType();
                if (t == typeof(System.Int32[]))
                {
                    standardSpectrum = (int[])value;
                }
                else
                {
                    standardSpectrum = JsonConvert.DeserializeObject<int[]>(value.ToString());
                }
            }
        }
        public int StandardCount
        {
            get
            {
                return standardCount;
            }
            set
            {
                standardCount = value;
            }
        }
        public DateTime PostInjection1DrawnTimestamp { get; set; }
        public DateTime PostInjection1CountedTimestamp { get; set; }
        public Object? PostInjection1Spectrum
        {
            get
            {
                return postInjection1Spectrum;
            }
            set
            {
                if (value is null)
                {
                    postInjection1Spectrum = new int[] { };
                    return;
                }
                Type t = value.GetType();
                if (t == typeof(System.Int32[]))
                {
                    postInjection1Spectrum = (int[])value;
                }
                else
                {
                    postInjection1Spectrum = JsonConvert.DeserializeObject<int[]>(value.ToString());
                }
            }
        }
        public int PostInjection1Count
        {
            get
            {
                return postInjection1Count;
            }
            set
            {
                postInjection1Count = value;
            }
        }
        public decimal? PostInjection1PHCT { get; set; }
        public decimal? PostInjection1NHCT { get; set; }
        public Boolean PostInjection1Included { get; set; }
        public DateTime? PostInjection2DrawnTimestamp { get; set; }
        public DateTime? PostInjection2CountedTimestamp { get; set; }
        public Object? PostInjection2Spectrum
        {
            get
            {
                return postInjection2Spectrum;
            }
            set
            {
                if (value is null)
                {
                    postInjection2Spectrum = new int[] { };
                    return;
                }
                Type t = value.GetType();
                if (t == typeof(System.Int32[]))
                {
                    postInjection2Spectrum = (int[])value;
                }
                else
                {
                    postInjection2Spectrum = JsonConvert.DeserializeObject<int[]>(value.ToString());
                }
            }
        }
        public int? PostInjection2Count
        {
            get
            {
                return postInjection2Count;
            }
            set
            {
                postInjection2Count = Convert.ToInt32(value == null ? 0 : value);
            }
        }
        public decimal? PostInjection2PHCT { get; set; }
        public decimal? PostInjection2NHCT { get; set; }      
        public Boolean PostInjection2Included { get; set; }
        public DateTime? PostInjection3DrawnTimestamp { get; set; }
        public DateTime? PostInjection3CountedTimestamp { get; set; }
        public Object? PostInjection3Spectrum
        {
            get
            {
                return postInjection3Spectrum;
            }
            set
            {
                if (value is null)
                {
                    postInjection3Spectrum = new int[] { };
                    return;
                }
                Type t = value.GetType();
                if (t == typeof(System.Int32[]))
                {
                    postInjection3Spectrum = (int[])value;
                }
                else
                {
                    postInjection3Spectrum = JsonConvert.DeserializeObject<int[]>(value.ToString());
                }
            }
        }
        public int? PostInjection3Count
        {
            get
            {
                return postInjection3Count;
            }
            set
            {
                postInjection3Count = Convert.ToInt32(value == null ? 0 : value);
            }
        }
        public decimal? PostInjection3PHCT { get; set; }
        public decimal? PostInjection3NHCT { get; set; }       
        public Boolean PostInjection3Included { get; set; }
        public DateTime? PostInjection4DrawnTimestamp { get; set; }
        public DateTime? PostInjection4CountedTimestamp { get; set; }
        public Object? PostInjection4Spectrum
        {
            get
            {
                return postInjection4Spectrum;
            }
            set
            {
                if (value is null)
                {
                    postInjection4Spectrum = new int[] { };
                    return;
                }
                Type t = value.GetType();
                if (t == typeof(System.Int32[]))
                {
                    postInjection4Spectrum = (int[])value;
                }
                else
                {
                    postInjection4Spectrum = JsonConvert.DeserializeObject<int[]>(value.ToString());
                }
            }
        }
        public int? PostInjection4Count
        {
            get
            {
                return postInjection4Count;
            }
            set
            {
                postInjection4Count = Convert.ToInt32(value == null ? 0 : value);
            }
        }
        public decimal? PostInjection4PHCT { get; set; }
        public decimal? PostInjection4NHCT { get; set; }          
        public Boolean PostInjection4Included { get; set; }
        public DateTime? PostInjection5DrawnTimestamp { get; set; }
        public DateTime? PostInjection5CountedTimestamp { get; set; }
        public Object? PostInjection5Spectrum
        {
            get
            {
                return postInjection5Spectrum;
            }
            set
            {
                if (value is null)
                {
                    postInjection5Spectrum = new int[] { };
                    return;
                }
                Type t = value.GetType();
                if (t == typeof(System.Int32[]))
                {
                    postInjection5Spectrum = (int[])value;
                }
                else
                {
                    postInjection5Spectrum = JsonConvert.DeserializeObject<int[]>(value.ToString());
                }
            }
        }
        public int? PostInjection5Count
        {
            get
            {
                return postInjection5Count;
            }
            set
            {
                postInjection5Count = Convert.ToInt32(value == null ? 0 : value);
            }
        }
        public decimal? PostInjection5PHCT { get; set; }
        public decimal? PostInjection5NHCT { get; set; }         
        public Boolean PostInjection5Included { get; set; }
        public string TestType { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }
        public bool WholeBlood { get; set; }
        public decimal AlbuminTransudationRate { get; set; }
        public decimal PHCTAVG { get; set; }
        public decimal NHCTAVG { get; set; }
        public decimal UBV { get; set; }
        public decimal IBV { get; set; }
        public decimal TBV { get; set; }
        public decimal TBVDeviation { get; set; }
        public decimal IPV { get; set; }
        public decimal PV { get; set; }
        public decimal PVDeviation { get; set; }
        public decimal IRBCV { get; set; }
        public decimal RBCV { get; set; }
        public decimal RBCVDeviation { get; set; }
    }
}