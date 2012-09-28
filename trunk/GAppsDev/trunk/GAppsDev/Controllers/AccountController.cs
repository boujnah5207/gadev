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
using GAppsDev.Models.ErrorModels;

namespace GAppsDev.Controllers
{
    [Authorize]
    public class AccountController : Controller
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
                else
                {
                    return View("Error", new ErrorModel("חשבון המשתמש שלך לא מורשה. אנא צור קשר עם מנהל המערכת."));
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
            Session.Remove("User");
            OpenIdUser user = (OpenIdUser)Session["User"];
            Response.Cookies[OpenIdMembershipService.LOGIN_COOKIE_NAME].Expires = DateTime.Now.AddDays(-1);
            //WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }
    }
}
