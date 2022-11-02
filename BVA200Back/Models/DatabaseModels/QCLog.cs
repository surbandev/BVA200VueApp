#nullable enable
using System;

namespace BVA200.Models
{
    public class QCLog
    {
        public Int32 ID { get; set; }
        public String TestType { get; set; }
        public DateTime Timestamp { get; set; }
        public int Counts { get; set; }
        public int CPM { get; set; }
        public Boolean Result { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}