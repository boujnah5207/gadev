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

namespace GAppsDev.ExtraClasses
{
    public class SendNotification
    {
        //public const string FROM_ADDRESS = "NOREPLY@pqdev.com";
        //public const string FROM_PASSWORD = "noreply50100200";

        //public void OrderApproved(Order order, string returnUrl)
        //{
        //    EmailMethods emailMethods = new EmailMethods(FROM_ADDRESS, Loc.Dic.OrdersSystem, FROM_PASSWORD);

        //    string emailSubject = String.Format("{0} {1} {2} {3} {4} {5}", Loc.Dic.Order, order.OrderNumber, Translation.Status((StatusType)order.StatusId), Loc.Dic.By, order.User.FirstName, order.User.LastName);
        //    StringBuilder emailBody = new StringBuilder();

        //    emailBody.AppendLine(emailSubject);
        //    emailBody.AppendLine();
        //    emailBody.AppendLine(String.Format("{0}: {1}", Loc.Dic.SeeDetailsAt, Url.Action("Details", "Orders", new { id = id }, "http")));

        //    emailMethods.sendGoogleEmail(orderFromDB.User.Email, orderFromDB.User.FirstName, emailSubject, emailBody.ToString());

        //}
    }
}