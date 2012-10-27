using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DB;

namespace GAppsDev.Models
{
    public class PrintOrderModel
    {
        public Order Order { get; set; }
        public List<Orders_OrderToItem> Items { get; set; }
    }
}