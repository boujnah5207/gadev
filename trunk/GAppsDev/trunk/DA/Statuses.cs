using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB;

namespace DA
{
    public enum StatusType
    {
        OrderCancelled = 0,
        Pending = 1,
        Declined = 2,
        PendingOrderCreator = 3,
        PartiallyApproved = 4,
        ApprovedPendingInvoice = 5,
        InvoiceScannedPendingOrderCreator = 6,
        InvoiceApprovedByOrderCreatorPendingFileExport = 7,
        InvoiceExportedToFile = 8,
        InvoiceExportedToFilePendingReceipt = 9,
        ReceiptScanned = 10
    }

    
}
