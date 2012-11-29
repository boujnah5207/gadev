using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DB;

namespace GAppsDev.Models
{
    public class CreateOrderModel
    {
        public bool IsFutureOrder { get; set; }

        public Order Order { get; set; }
        public string ItemsString { get; set; }

        public int? BudgetAllocationId { get; set; }
        public List<OrderAllocation> Allocations { get; set; }

        public CreateOrderModel()
        {
            Order = new Order();
        }
    }

    public class OrderAllocation
    {
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public int AllocationId { get; set; }
        public int? MonthId { get; set; }
        public decimal Amount { get; set; }
    }
}