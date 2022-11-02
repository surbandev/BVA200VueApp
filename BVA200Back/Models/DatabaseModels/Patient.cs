using System;

namespace BVA200.Models
{
    public class Patient
    {
        public string ID { get; set; }
        public string PublicID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Sex { get; set; }
        public DateTime DOB { get; set; }
        public bool Amputee { get; set; }
        public bool Pregnant { get; set; }
        public string BloodType { get; set; }
        public decimal Height { get; set; }
        public decimal Weight { get; set; }


        public string FullName
        {
            get
            {
                return $"{LastName} {FirstName}";
            }
        }
    }
}
