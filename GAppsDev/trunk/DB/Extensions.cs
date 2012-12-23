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
        [DisplayFormat(DataFormatString = "{0:0.##}")]
        public decimal Price { get; set; }

        [StringLength(250)]
        public string Notes { get; set; }

        [StringLength(250)]
        public string OrderApproverNotes { get; set; }
    }

    [MetadataType(typeof(AllocationToMonthMetaData))]
    public partial class AllocationToMonth
    {
        public Type Metadata { get { return typeof(AllocationToMonthMetaData); } }
    }

    public class AllocationToMonthMetaData
    {
        [DisplayFormat(DataFormatString = "{0:0.##}")]
        public decimal Amount { get; set; }

    }

    public class SuppliersMetaData
    {
        [LocalizedNumberString]
        [LocalizedName("ExternalId")]
        public int ExternalId { get; set; }
    }

    [MetadataType(typeof(SuppliersMetaData))]
    public partial class Suppliers
    {
        public Type Metadata { get { return typeof(SuppliersMetaData); } }
    }

    [MetadataType(typeof(Users_ApprovalRoutesMetaData))]
    public partial class Users_ApprovalRoutes
    {
        public Type Metadata { get { return typeof(Users_ApprovalRoutesMetaData); } }
    }

    public class Users_ApprovalRoutesMetaData
    {
        [LocalizedName("Name")]
        public decimal Name { get; set; }
    }

    //
    // Helper methods
    //
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

    public partial class User
    {
        public string FullName
        {
            get
            {
                return String.Format("{0} {1}", this.FirstName, this.LastName);
            }
        }
    }
}