using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DB;

namespace GAppsDev.Models.Search
{
    public class OrdersSearchValuesModel
    {
        public int? UserId { get; set; }
        public int? SupplierId { get; set; }
        public int? StatusId { get; set; }

        public int? PriceMin { get; set; }
        public int? PriceMax { get; set; }
        public DateTime? CreationMin { get; set; }
        public DateTime? CreationMax { get; set; }

        public string NoteText { get; set; }
    }
}