using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BL;
using DB;
using Resources;

namespace GAppsDev.Models
{
    public class UploadReceiptModel
    {
        public bool isUpdate = false;

        [LocalizedFile(Validations.MAX_FILE_SIZE)]
        public HttpPostedFileBase File { get; set; }
    }
}