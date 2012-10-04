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

        [Display(Name = "סכום כולל")]
        public decimal Price { get; set; }

        [Display(Name = "סטאטוס")]
        public int StatusId { get; set; }

        [Display(Name = "שם המזמין")]
        public int UserId { get; set; }

        [Display(Name = "הערות יוצר ההזמנה")]
        public string Notes { get; set; }

        [Display(Name = "הערות מאשר ההזמנה")]
        public string OrderApproverNotes { get; set; }
    }

}