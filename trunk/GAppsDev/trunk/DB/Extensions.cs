using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DB
{
    [MetadataType(typeof(OrderMetaData))]
    public partial class Order
    {
        public Type Metadata { get { return typeof(OrderMetaData); } }
    }

    public class OrderMetaData
    {
        [Display(Name="שם ספק")]
        public int SupplierId { get; set; }
        
        [Display(Name = "תאריך יצירה")]
        public DateTime CreationDate { get; set; }

        [Display(Name = "סכום")]
        public decimal Price { get; set; }

        [Display(Name = "סטאטוס")]
        public int StatusId { get; set; }
    }

}