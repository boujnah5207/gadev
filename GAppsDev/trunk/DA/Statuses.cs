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
        InvoiceScannedPendingReceipt = 5,
        ReceiptScanned = 6,
        PendingOrderCreator = 7,
    }
}
