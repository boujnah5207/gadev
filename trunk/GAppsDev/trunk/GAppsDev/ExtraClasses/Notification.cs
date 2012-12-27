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

namespace GAppsDev.ExtraClasses
{
    public class SendNotifications
    {
        //public const string FROM_ADDRESS = "NOREPLY@pqdev.com";
        //public const string FROM_PASSWORD = "noreply50100200";

        //public void OrderApproved(Order order, UrlHelper url)
        //{
        //    EmailMethods emailMethods = new EmailMethods(FROM_ADDRESS, Loc.Dic.OrdersSystem, FROM_PASSWORD);

        //    string emailSubject = String.Format("{0} {1} {2} {3} {4} {5}", Loc.Dic.Order, order.OrderNumber, Translation.Status((StatusType)order.StatusId), Loc.Dic.By, order.User.FirstName, order.User.LastName);
        //    StringBuilder emailBody = new StringBuilder();

        //    emailBody.AppendLine(emailSubject);
        //    emailBody.AppendLine();
        //    emailBody.AppendLine(String.Format("{0}: {1}", Loc.Dic.SeeDetailsAt, url.Action("Details", "Orders", new { id = order.Id }, "http")));
        //    emailBody.AppendLine();
        //    emailBody.AppendLine();
        //    emailBody.AppendLine(String.Format("{0}:\n{1}", Loc.Dic.RemoveNotificationsMessage, url.Action("RemoveNotifications", "Users", new { id = order.User.NotificationsCode }, "http")));

        //    emailMethods.sendGoogleEmail(order.User.Email, order.User.FirstName, emailSubject, emailBody.ToString());
        //}
    }
}