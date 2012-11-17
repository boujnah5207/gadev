using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DB;

namespace GAppsDev.Models
{
    public class CreateOrderModel
    {
        bool IsFutureOrder = false;

        public Order Order { get; set; }
        public string ItemsString { get; set; }

        public int? AllocationId { get; set; }
        public List<OrderAllocation> Allocations { get; set; }

        public CreateOrderModel()
        {
            Order = new Order();
        }
    }

    public class OrderAllocation
    {
        public bool IsActive { get; set; }
        public int AllocationId { get; set; }
        public int MonthId { get; set; }
        public decimal Amount { get; set; }
    }
}