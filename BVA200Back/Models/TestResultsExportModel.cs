#nullable enable

using System;
using Newtonsoft.Json;

namespace BVA200.Models
{
    public class ExportModel
    {
        private int[] rtr;
        public Object? RequestedIDs
        {
            get
            {
                return rtr;
            }
            set
            {
                if (value is null)
                {
                    rtr = null;
                    return;
                }
                Type t = value.GetType();
                if (t == typeof(System.Int32[]))
                {
                    rtr = (int[])value;
                }
                else
                {
                    rtr = JsonConvert.DeserializeObject<int[]>(value.ToString());
                }
            }
        }
        public bool? AllResults { get; set; }
        public String? Format { get; set; }//excel, csv, pdf, word
        public string? Printer { get; set; }
    }
}