using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DB;

namespace GAppsDev.Models.InventoryItemModel
{
    public class ManualCreateInventoryItemModel
    {
        public Inventory inventoryItem { get; set; }
        public Orders_Items item { get; set; }
        public ManualCreateInventoryItemModel()
        {
            inventoryItem = new Inventory();
            item = new Orders_Items();
        }
    }

}