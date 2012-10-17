using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GAppsDev.Models.Search
{
    public class OrdersSearchFormModel
    {
        public OrdersSearchValuesModel SearchValues { get; set; }

        public SelectList UsersList { get; set; }
        public SelectList SuppliersList { get; set; }
        public SelectList StatusesList { get; set; }
    }
}