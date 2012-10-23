﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB;

namespace DA
{
    public enum StatusType : int
    {
        Pending = 1,
        Declined = 2,
        ApprovedPendingInvoice = 3,
        InvoiceScannedPendingReceipt = 4,
        ReceiptScanned = 5,
        PendingOrderCreator = 6
    }
}
