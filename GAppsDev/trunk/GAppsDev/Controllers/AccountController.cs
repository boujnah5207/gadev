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
                    ViewBag.ErrorMessage = "חשבון המשתמש אינו רשום במערכת";
                    return View("Error");
                }
            }


            //bool sucsess = Membership.ValidateUser("netivot@org", "123456");

            //string s = HttpContext.User.Identity.AuthenticationType;
            //string h = User.Identity.AuthenticationType;

            //using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, @"https://matnasim.co.il"))
            //{
            //    // validate the credentials
            //    bool isValid = pc.ValidateCredentials("netivot@org", "123456");
            //}

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

            //HttpCookie myCookie = new HttpCookie(OpenIdMembershipService.LOGIN_COOKIE_NAME);
            //myCookie.Expires = DateTime.Now.AddDays(-1d);
            //Response.Cookies.Add(myCookie);

            return View();
        }
    }
}
