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
        OrderCancelled = 0,
        Pending = 1,
        Declined = 2,
        PendingOrderCreator = 3,
        PartiallyApproved = 4,
        ApprovedPendingInvoice = 5,
        InvoiceScannedPendingOrderCreator = 6,
        InvoiceApprovedByOrderCreatorPendingFileExport = 7,
        InvoiceExportedToFilePendingReceipt = 8,
        ReceiptScanned = 9
    }
}
