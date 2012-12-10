using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DA;

namespace GAppsDev.Resources
{
    public static class Translation
    {
        public static class OrderStatuses
        {
            public static string GetStatusName(StatusType status)
            {
                switch (status)
                {
                    case StatusType.OrderCancelled:
                        return Loc.Dic.Status_OrderCancelled;
                    case StatusType.Pending:
                        return Loc.Dic.Status_Pending;
                    case StatusType.Declined:
                        return Loc.Dic.Status_Declined;
                    case StatusType.PendingOrderCreator:
                        return Loc.Dic.Status_PendingOrderCreator;
                    case StatusType.PartiallyApproved:
                        return Loc.Dic.Status_PartiallyApproved;
                    case StatusType.ApprovedPendingInvoice:
                        return Loc.Dic.Status_OrderCancelled;
                    case StatusType.InvoiceScannedPendingOrderCreator:
                        return Loc.Dic.Status_OrderCancelled;
                    case StatusType.InvoiceApprovedByOrderCreatorPendingFileExport:
                        return Loc.Dic.Status_OrderCancelled;
                    case StatusType.InvoiceExportedToFile:
                        return Loc.Dic.Status_OrderCancelled;
                    case StatusType.InvoiceExportedToFilePendingReceipt:
                        return Loc.Dic.Status_OrderCancelled;
                    case StatusType.ReceiptScanned:
                        return Loc.Dic.Status_OrderCancelled;
                    default:
                        return Loc.Dic.Status_OrderCancelled;
                }
            }
        }
    }
}