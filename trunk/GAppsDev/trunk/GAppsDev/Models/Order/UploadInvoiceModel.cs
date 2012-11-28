using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GAppsDev.Models
{
    public class UploadInvoiceModel
    {
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime ValueDate { get; set; }
    }
}