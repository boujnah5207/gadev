using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using GAppsDev.OpenIdService;
using Mvc4.OpenId.Sample.Security;
using DotNetOpenAuth.Messaging;
using WebMatrix.WebData;
using System.Text;

namespace GAppsDev.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly OpenIdMembershipService openidemembership;

        public AccountController()
        {
            openidemembership = new OpenIdMembershipService();
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            LogInResult result;
            result = openidemembership.GetUser();

            if (result.IsAuthenticated)
            {
                if (result.IsRegistered || result.IsNewUser)
                {
                    var cookie = openidemembership.CreateFormsAuthenticationCookie(result.User);
                    HttpContext.Response.Cookies.Add(cookie);
                    return new RedirectResult(Request.Params["ReturnUrl"] ?? "/");
                }
                else if (result.IsCanceled)
                {
                    ViewBag.ErrorMessage = "חשבון משתמש זה בוטל, אנא פנה למנהל המערכת";
                    return View("Error");
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("חשבון המשתמש אינו רשום במערכת");
                    builder.AppendLine("<br />");
                    builder.AppendLine("אנא צא מהחשבון הנוכחי והיכנס עם הרשאות מערכת הרכש");

                    ViewBag.ErrorMessage = builder.ToString();
                    return View("Error");
                }
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(string openid_identifier)
        {
            var response = openidemembership.ValidateAtOpenIdProvider(openid_identifier);
            if (response != null)
            {
                return response.RedirectingResponse.AsActionResult();
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult LogOff()
        {
            ViewBag.Title = Loc.Dic.LogOff;
            ViewBag.Message = Loc.Dic.LogOffWasSuccessful;
            ViewBag.Align = (CurrentUser.LanguageCode == "he" || CurrentUser.LanguageCode == "ar") ? "right" : "left";

            Session.Clear();

            string[] myCookies = Request.Cookies.AllKeys;
            foreach (string cookie in myCookies)
            {
                Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1);
            }

            return View();
        }
    }
}
