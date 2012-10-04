using DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GAppsDev.Models
{
    public class OrderModel
    {
        public Order Order { get; set; }
        public List<Orders_OrderToItem> OrderToITem { get; set; }
    }
}