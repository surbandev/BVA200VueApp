using System;

namespace BVA200.Models
{
    public class UserSession
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public String UserSessionGUID { get; set; }//Dapper doesn't play nice with converting SQLite Text to Guid datatype. Have to just use string.
        public DateTime LoggedIn { get; set; }
        public DateTime LoggedOut { get; set; }
        public DateTime LastActive { get; set; }
        public bool IsAdmin{get;set;}
    }
}
