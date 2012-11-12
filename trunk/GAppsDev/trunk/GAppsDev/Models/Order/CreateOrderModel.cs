﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DB;

namespace GAppsDev.Models
{
    public class CreateOrderModel
    {
        public Order Order { get; set; }
        public string ItemsString { get; set; }
        public List<OrderAllocation> Allocations { get; set; }
    }

    public class OrderAllocation
    {
        public bool IsActive { get; set; }
        public int AllocationId { get; set; }
        public int MonthId { get; set; }
        public int Amount { get; set; }
    }
}