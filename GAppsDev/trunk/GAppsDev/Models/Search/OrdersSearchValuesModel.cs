using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DB;

namespace GAppsDev.Models.Search
{
    public class OrdersSearchValuesModel
    {
        public List<Order> Matches { get; set; }

        public SelectList UsersList { get; set; }
        public SelectList BudgetsList { get; set; }
        public SelectList SuppliersList { get; set; }
        public SelectList StatusesList { get; set; }
        public SelectList AllocationsList { get; set; }

        public bool HideUserField { get; set; }
        public bool HideStatusField { get; set; }
        public bool HideSupplierField { get; set; }

        public int? UserId { get; set; }
        public int? OrderNumber { get; set; }
        public int? BudgetId { get; set; }
        public int? SupplierId { get; set; }
        public int? StatusId { get; set; }
        public int? AllocationId { get; set; }

        public int? PriceMin { get; set; }
        public int? PriceMax { get; set; }
        public DateTime? CreationMin { get; set; }
        public DateTime? CreationMax { get; set; }

        public string NoteText { get; set; }
    }
}