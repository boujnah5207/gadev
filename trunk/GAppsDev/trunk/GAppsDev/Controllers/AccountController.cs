﻿using System;
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
                    return View("Error_NoLayout", new ErrorModel("חשבון משתשתמש זה בוטל. אנא פנה למנהל המערכת."));
                }
                else
                {
                    return View("Error_NoLayout", new ErrorModel("חשבון משתשתמש זה אינו רשום במערכת."));
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

            HttpCookie myCookie = new HttpCookie(OpenIdMembershipService.LOGIN_COOKIE_NAME);
            myCookie.Expires = DateTime.Now.AddDays(-1d);
            Response.Cookies.Add(myCookie);

            return View();
        }
    }
}