using System;
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
        PartiallyApproved = 3,
        ApprovedPendingInvoice = 4,
        InvoiceScannedPendingOrderCreator = 5,
        InvoiceApprovedByOrderCreatorPendingFileExport = 6,
        InvoiceExportedToFilePendingReceipt = 7,
        ReceiptScanned = 8,
        PendingOrderCreator = 9,
        OrderCancelled = 10
    }
}
