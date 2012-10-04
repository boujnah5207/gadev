using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GAppsDev.Models.SupplierModels
{
    public class AjaxSupplier
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int VAT_Number { get; set; }
        public string Phone_Number { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string Customer_Number { get; set; }
        public string Additional_Phone { get; set; }
        public string EMail { get; set; }
        public string Fax { get; set; }
        public string Activity_Hours { get; set; }
        public string Branch_Line { get; set; }
        public string Presentor_name { get; set; }
        public string Crew_Number { get; set; }
        public string Notes { get; set; }
        public DateTime CreationDate { get; set; }
    }
}