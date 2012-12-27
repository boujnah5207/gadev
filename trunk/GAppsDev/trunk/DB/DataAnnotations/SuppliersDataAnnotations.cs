using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DB
{
    [MetadataType(typeof(SupplierMetaData))]
    public partial class Supplier
    {
        public Type Metadata { get { return typeof(SupplierMetaData); } }
    }

    public class SupplierMetaData
    {
        [LocalizedNumberString]
        [LocalizedName("ExternalId")]
        public int ExternalId { get; set; }

        [LocalizedName("VAT_Number")]
        public int VAT_Number { get; set; }

        [LocalizedName("CreationDate")]
        public DateTime CreationDate { get; set; }

        [LocalizedName("Name")]
        public string Name { get; set; }

        [LocalizedName("Address")]
        public string Address { get; set; }

        [LocalizedName("City")]
        public string City { get; set; }

        [LocalizedName("Phone_Number")]
        public string Phone_Number { get; set; }

        [LocalizedName("Customer_Number")]
        public string Customer_Number { get; set; }

        [LocalizedName("Additional_Phone")]
        public string Additional_Phone { get; set; }

        [LocalizedName("EMail")]
        public string EMail { get; set; }

        [LocalizedName("Fax")]
        public string Fax { get; set; }

        [LocalizedName("Activity_Hours")]
        public string Activity_Hours { get; set; }

        [LocalizedName("Branch_line")]
        public string Branch_line { get; set; }

        [LocalizedName("Presentor_name")]
        public string Presentor_name { get; set; }

        [LocalizedName("Crew_Number")]
        public string Crew_Number { get; set; }

        [LocalizedName("Notes")]
        public string Notes { get; set; }

        [LocalizedName("IsCanceled")]
        public bool IsCanceled { get; set; }
    }
}
