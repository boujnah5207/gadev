﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DB;

namespace GAppsDev.Models
{
    public class AddToInventoryModel
    {
        public int OrderId { get; set; }
        public List<Orders_OrderToItem> OrderItems { get; set; }
        public SelectList LocationsList { get; set; }
        public List<List<Inventory>> InventoryItems { get; set; }
    }
}