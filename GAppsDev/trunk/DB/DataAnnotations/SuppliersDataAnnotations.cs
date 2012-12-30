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
        [LocalizedName("Field_ExternalId")]
        public int ExternalId { get; set; }

        [LocalizedName("Field_VAT_Number")]
        public int VAT_Number { get; set; }

        [LocalizedName("Field_CreationDate")]
        public DateTime CreationDate { get; set; }

        [LocalizedName("Field_Name")]
        public string Name { get; set; }

        [LocalizedName("Field_Address")]
        public string Address { get; set; }

        [LocalizedName("Field_City")]
        public string City { get; set; }

        [LocalizedName("Field_Phone_Number")]
        public string Phone_Number { get; set; }

        [LocalizedName("Field_Customer_Number")]
        public string Customer_Number { get; set; }

        [LocalizedName("Field_Additional_Phone")]
        public string Additional_Phone { get; set; }

        [LocalizedName("Field_EMail")]
        public string EMail { get; set; }

        [LocalizedName("Field_Fax")]
        public string Fax { get; set; }

        [LocalizedName("Field_Activity_Hours")]
        public string Activity_Hours { get; set; }

        [LocalizedName("Field_Branch_line")]
        public string Branch_line { get; set; }

        [LocalizedName("Field_Presentor_name")]
        public string Presentor_name { get; set; }

        [LocalizedName("Field_Crew_Number")]
        public string Crew_Number { get; set; }

        [LocalizedName("Field_Notes")]
        public string Notes { get; set; }

        [LocalizedName("Field_IsCanceled")]
        public bool IsCanceled { get; set; }
    }
}
