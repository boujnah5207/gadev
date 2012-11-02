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
        InvoiceApprovedByOrderCreatorPendingReceipt = 6,
        ReceiptScanned = 7,
        PendingOrderCreator = 8,
        OrderCancelled = 9
    }
}
