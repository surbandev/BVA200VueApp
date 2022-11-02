using System;

namespace BVA200.Models
{
    public class QCLogGridModel
    {
        public int ID { get; set; }
        public string Timestamp { get; set; }
        public string TestType{ get; set; }
        public Boolean Result { get; set; }
        public int CreatedBy { get; set; }

        public QCLogGridModel(QCLog ql)
        {
            this.ID = Convert.ToInt32(ql.ID);
            this.Timestamp = Convert.ToDateTime(ql.Timestamp).ToShortDateString();
            this.TestType = ql.TestType;
            this.Result = ql.Result;
            this.CreatedBy = Convert.ToInt32(ql.CreatedBy);
        }
    }
}