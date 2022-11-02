using System;
using System.Collections.Generic;

namespace BVA200.Models
{
    public class UserConfigurationViewModel
    {
        public string CurrentUserSession { get; set; }
        public CurrentTest CurrentTest { get; set; }
        public string PatientID { get; set; }
        public IEnumerable<User> Users { get; set; }
        public string DoseInjectionTimestamp { get; set; }
    }
}