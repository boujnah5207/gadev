﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;

namespace GAppsDev
{
    public static class Translation
    {
        public static string Status(StatusType status)
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
                    return Loc.Dic.Status_ApprovedPendingInvoice;
                case StatusType.InvoiceScannedPendingOrderCreator:
                    return Loc.Dic.Status_PendingInvoiceApproval;
                case StatusType.InvoiceApprovedByOrderCreatorPendingFileExport:
                    return Loc.Dic.Status_PendingExport;
                case StatusType.InvoiceExportedToFile:
                    return Loc.Dic.Status_ExportedToFile;
                case StatusType.InvoiceExportedToFilePendingReceipt:
                    return Loc.Dic.Status_PendingReceipt;
                case StatusType.ReceiptScanned:
                    return Loc.Dic.Status_ReceiptScanned;

                default:
                    return Loc.Dic.Status_Unknown;
            }
        }
        public static string OrderHistoryAction(HistoryActions action)
        {
            switch (action)
            {
                case HistoryActions.Created:
                    return Loc.Dic.Created;

                case HistoryActions.Edited:
                    return Loc.Dic.Edited;

                case HistoryActions.PartiallyApproved:
                    return Loc.Dic.PartiallyApproved;

                case HistoryActions.Declined:
                    return Loc.Dic.Declined;

                case HistoryActions.ReturnedToCreator:
                    return Loc.Dic.ReturnedToCreator;

                case HistoryActions.PassedApprovalRoute:
                    return Loc.Dic.PassedApprovalRoute;

                case HistoryActions.Canceled:
                    return Loc.Dic.Canceled;

                case HistoryActions.InvoiceScanned:
                    return Loc.Dic.InvoiceScanned;

                case HistoryActions.InvoiceApproved:
                    return Loc.Dic.InvoiceApproved;

                case HistoryActions.ExportedToFile:
                    return Loc.Dic.ExportedToFile;

                case HistoryActions.ReceiptScanned:
                    return Loc.Dic.ReceiptScanned;

                case HistoryActions.AddedToInventory:
                    return Loc.Dic.AddedToInventory;

                case HistoryActions.OrderPrinted:
                    return Loc.Dic.OrderPrinted;

                default:
                    return Loc.Dic.Unknown;
            }
        }
        public static string Month(int month)
        {
            switch (month)
            {
                case 1:
                    return Loc.Dic.January;
                case 2:
                    return Loc.Dic.February;
                case 3:
                    return Loc.Dic.March;
                case 4:
                    return Loc.Dic.April;
                case 5:
                    return Loc.Dic.May;
                case 6:
                    return Loc.Dic.June;
                case 7:
                    return Loc.Dic.July;
                case 8:
                    return Loc.Dic.August;
                case 9:
                    return Loc.Dic.September;
                case 10:
                    return Loc.Dic.October;
                case 11:
                    return Loc.Dic.November;
                case 12:
                    return Loc.Dic.December;
                default:
                    return "Invalid Month Number";
            }
        }
    }
}