using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using BL;

namespace GAppsDev.Models
{
    public class UploadInvoiceModel
    {
        public bool isUpdate = false;

        [LocalizedNumberStringAttribute]
        [LocalizedRequired]
        [LocalizedName("InvoiceNumber")]
        [LocalizedMaxLength(20)]
        public string InvoiceNumber { get; set; }

        [LocalizedRequired]
        [LocalizedName("InvoiceDate")]
        public DateTime InvoiceDate { get; set; }

        [LocalizedRequired]
        [LocalizedName("ValueDate")]
        public DateTime ValueDate { get; set; }

        [LocalizedFile(Validations.MAX_FILE_SIZE)]
        public HttpPostedFileBase File { get; set; }
    }
}