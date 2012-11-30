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