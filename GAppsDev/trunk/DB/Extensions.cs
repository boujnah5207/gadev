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
        [DisplayFormat(DataFormatString = "{0:0}")]
        public decimal Price { get; set; }
    }

    [MetadataType(typeof(AllocationToMonthMetaData))]
    public partial class AllocationToMonth
    {
        public Type Metadata { get { return typeof(AllocationToMonthMetaData); } }
    }

    public class AllocationToMonthMetaData
    {
        [DisplayFormat(DataFormatString = "{0:0}")]
        public decimal Amount { get; set; }
        
    }

    public partial class Budgets_Allocations
    {
        public string DisplayName
        {
            get
            {
                return String.Format("{0} - {1}", this.ExternalId, this.Name);
            }
        }
    }
}