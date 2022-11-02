#nullable enable

namespace BVA200.Models
{
    /*
        The FirstName & LastName fields are in reference to the Technician (I.E. User)
        who preformed the BVA
    */
    public class SingleTestResult : TestResult
    {
        public string FirstName{get;set;}
        public string LastName{get;set;}
    }
}