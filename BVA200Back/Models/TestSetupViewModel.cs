namespace BVA200.Models
{
    public class TestSetupViewModel
    {
        public string UserSessionGUID { get; set; }
        public CurrentTest CurrentTest { get; set; }
        public string PatientID { get; set; }
        public string DoseInjectionTimestamp { get; set; }
    }
}