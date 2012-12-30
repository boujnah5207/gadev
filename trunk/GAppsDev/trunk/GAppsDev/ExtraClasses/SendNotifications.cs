using BaseLibraries;
using DA;
using DB;
using GAppsDev.OpenIdService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace GAppsDev.ExtraClasses
{
    public static class SendNotifications
    {
        public const string FROM_ADDRESS = "NOREPLY@pqdev.com";
        public const string FROM_PASSWORD = "noreply50100200";

        public const string EXPORT_ORDERS_FILE_NAME = "MOVEIN.DAT";

        public static void OrderStatusChanged(Order order, OpenIdUser updater, UrlHelper url)
        {
            EmailMethods emailMethods = new EmailMethods(FROM_ADDRESS, Loc.Dic.OrdersSystem, FROM_PASSWORD);

            string emailSubject = String.Format("{0} {1} {2} {3} {4}", Loc.Dic.Order, order.OrderNumber, Translation.Status((StatusType)order.StatusId), Loc.Dic.By, updater.FullName);
            StringBuilder emailBody = new StringBuilder();

            emailBody.AppendLine(emailSubject);
            emailBody.AppendLine();
            emailBody.AppendLine(String.Format("{0}: {1}", Loc.Dic.SeeDetailsAt, url.Action("Details", "Orders", new { id = order.Id }, "http")));

            SendNotification(url, order.User.Email, order.User.FirstName, emailSubject, emailBody, order.User.NotificationEmail, order.User.NotificationCode);
        }

        public static void OrderPendingApproval(Order order, UrlHelper url)
        {
            EmailMethods emailMethods = new EmailMethods(FROM_ADDRESS, Loc.Dic.OrdersSystem, FROM_PASSWORD);

            string emailSubject = String.Format("{0} {1} {2} {3} {4} {5}", Loc.Dic.Order, order.OrderNumber, Loc.Dic.OrderFrom, order.User.FirstName, order.User.LastName, Loc.Dic.PendingYourApproval);
            StringBuilder emailBody = new StringBuilder();

            emailBody.AppendLine(emailSubject);
            emailBody.AppendLine();
            emailBody.AppendLine(String.Format("{0}: {1}", Loc.Dic.ModifyOrderStatusAt, url.Action("ModifyStatus", "Orders", new { id = order.Id }, "http")));

            SendNotification(url, order.User1.Email, order.User1.FirstName, emailSubject, emailBody, order.User1.NotificationEmail, order.User1.NotificationCode);
        }

        public static void OrdersExported(OpenIdUser exporter, UrlHelper url, int itemsExported, byte[] exportedFile)
        {
            EmailMethods emailMethods = new EmailMethods(FROM_ADDRESS, Loc.Dic.OrdersSystem, FROM_PASSWORD);

            string emailSubject = String.Format("{0} {1}", itemsExported, Loc.Dic.Email_OrdersWereExported);
            StringBuilder emailBody = new StringBuilder();
            emailBody.AppendLine(emailSubject);
            emailBody.AppendLine();
            emailBody.AppendLine(Loc.Dic.Email_CopyOfExportedFileIsAttached);

            SendNotification(url, exporter.Email, exporter.FirstName, emailSubject, emailBody, exporter.NotificationEmail, exporter.NotificationCode, exportedFile, EXPORT_ORDERS_FILE_NAME);
        }

        private static void SendNotification(UrlHelper url, string email, string fullName, string subject, StringBuilder body, string secondaryMail, string notificationCode, byte[] attachmentFile = null, string attachmentName = null)
        {
            EmailMethods emailMethods = new EmailMethods(FROM_ADDRESS, Loc.Dic.OrdersSystem, FROM_PASSWORD);

            emailMethods.sendGoogleEmail(email, fullName, subject, body.ToString(), attachmentFile, attachmentName);
            if (secondaryMail != null)
            {
                body.AppendLine();
                body.AppendLine();
                body.AppendLine(String.Format("{0}:\n{1}", Loc.Dic.RemoveNotificationsMessage, url.Action("RemoveNotifications", "Users", new { id = notificationCode }, "http")));

                emailMethods.sendGoogleEmail(secondaryMail, fullName, subject, body.ToString(), attachmentFile, attachmentName);
            }
        }
    }
}