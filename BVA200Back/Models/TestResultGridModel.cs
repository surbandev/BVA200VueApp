using System;

namespace BVA200.Models
{
    public class TestResultGridModel
    {
        public int ID { get; set; }
        public string PatientID { get; set; }
        public string TestDate { get; set; }
        public decimal TBV { get; set; }
        public decimal TBVDeviation { get; set; }

        public TestResultGridModel(TestResult tr)
        {
            this.ID = Convert.ToInt32(tr.ID);
            this.PatientID = tr.PatientID;
            this.TestDate = Convert.ToDateTime(tr.Created).ToShortDateString();
            this.TBV = tr.TBV;
            this.TBVDeviation = tr.TBVDeviation;
        }
    }
}