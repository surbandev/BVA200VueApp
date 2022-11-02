using System.Collections.Generic;

namespace BVA200.Models
{
    public class LoginModel
    {
        public string UserSessionGUID { get; set; }
        public int SessionTimeout { get; set; }
    }
}