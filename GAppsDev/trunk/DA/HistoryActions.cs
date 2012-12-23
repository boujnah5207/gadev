using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB;

namespace DA
{
    public enum HistoryActions
    {
        Created = 1,
        Edited = 2,
        PartiallyApproved = 3,
        Declined = 4,
        ReturnedToCreator = 5,
        PassedApprovalRoute = 6,
        Canceled = 7,
        InvoiceScanned = 8,
        InvoiceApproved = 9,
        ExportedToFile = 10,
        ReceiptScanned = 11,
        AddedToInventory = 12,
        OrderPrinted = 13
    }
}
