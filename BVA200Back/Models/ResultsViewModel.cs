using System.Collections.Generic;

namespace BVA200.Models{
    public class ResultsViewModel{
        public CurrentTest CurrentTest { get; set; }
        public string UserSessionGUID { get; set; }
        public int PatientID { get; set; }
    }
}