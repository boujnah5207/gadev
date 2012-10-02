﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DA;
using DB;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;
using GAppsDev.OpenIdService;

namespace Mvc4.OpenId.Sample.Security
{
    internal interface IOpenIdMembershipService
    {
        IAuthenticationRequest ValidateAtOpenIdProvider(string openIdIdentifier);
        LogInResult GetUser();
    }

    public class OpenIdMembershipService : IOpenIdMembershipService
    {
        public const string LOGIN_COOKIE_NAME = "GAppsDev_Login";
        private readonly OpenIdRelyingParty openId;

        public OpenIdMembershipService()
        {
            openId = new OpenIdRelyingParty();
        }

        public IAuthenticationRequest ValidateAtOpenIdProvider(string openIdIdentifier)
        {
            IAuthenticationRequest openIdRequest = openId.CreateRequest(Identifier.Parse(openIdIdentifier));
            
            var fetch = new FetchRequest();
            fetch.Attributes.AddRequired(WellKnownAttributes.Contact.Email);
            fetch.Attributes.AddRequired(WellKnownAttributes.Name.First);
            fetch.Attributes.AddRequired(WellKnownAttributes.Name.Last);
            openIdRequest.AddExtension(fetch);

            return openIdRequest;
        }

        public LogInResult GetUser()
        {
            LogInResult logInResult = new LogInResult();
            IAuthenticationResponse openIdResponse = openId.GetResponse();
            if (openIdResponse.IsSuccessful())
            {
                logInResult = ResponseIntoUser(openIdResponse);
            }
            return logInResult;
        }

        private LogInResult ResponseIntoUser(IAuthenticationResponse response)
        {
            LogInResult logInResult = new LogInResult();
            logInResult.IsAuthenticated = true;

            var fetchResponse = response.GetExtension<FetchResponse>();
            if (fetchResponse != null)
            {
                logInResult.User = new OpenIdUser(fetchResponse, response.ClaimedIdentifier);

                using(UserRepository userRep = new UserRepository())
                using (PendingUserRepository pendingUserRep = new PendingUserRepository())
                {
                    User user = userRep.GetList().SingleOrDefault(x => x.Email == logInResult.User.Email);
                    if (user != null)
                    {
                        logInResult.IsRegistered = true;
                        logInResult.User.UserId = user.Id;
                        return logInResult;
                    }
                    else
                    {
                        PendingUser pendingUser = pendingUserRep.GetList().SingleOrDefault(x => x.Email == logInResult.User.Email);

                        if (pendingUser != null)
                        {
                            User newUser = new User()
                            {
                                CompanyId = pendingUser.CompanyId,
                                Email = logInResult.User.Email,
                                FirstName = logInResult.User.FirstName,
                                LastName = logInResult.User.LastName,
                                CreationTime = DateTime.Now,
                                LastLogInTime = DateTime.Now,
                                Roles = pendingUser.Roles
                            };

                            try
                            {
                                userRep.Create(newUser);
                            }
                            catch
                            {
                                return logInResult;
                            }

                            logInResult.User.UserId = newUser.Id;
                            pendingUserRep.Delete(pendingUser.Id);

                            logInResult.IsNewUser = true;
                            logInResult.IsRegistered = true;
                            return logInResult;
                        }
                        else
                        {
                            return logInResult;
                        }
                    }
                }
            }
            else
            {
                return logInResult;
            }
        }

        public HttpCookie CreateFormsAuthenticationCookie(OpenIdUser user)
        {
            Random rand = new Random();
            int randomInt = rand.Next(0, int.MaxValue);
            string hashValue = MD5Encryptor.GetHash(randomInt.ToString());

            using (CookiesRepository cookiesRep = new CookiesRepository())
            {
                    Cookies existingCookie = cookiesRep.GetList().FirstOrDefault(x => x.UserId == user.UserId);

                    if (existingCookie != null)
                    {
                        if (cookiesRep.Delete(existingCookie.Id) == false)
                            return null;
                    }
                    Cookies newCookie = new Cookies()
                    {
                        UserId = user.UserId,
                        HashValue = hashValue
                    };
                    
                    if(cookiesRep.Create(newCookie) == false)
                        return null;
            }

            //var ticket = new FormsAuthenticationTicket(1, user.FullName, DateTime.Now, DateTime.Now.AddDays(7), true, user.GetCookieString(hashValue));
            //var encrypted = FormsAuthentication.Encrypt(ticket).ToString();
            var cookie = new HttpCookie(LOGIN_COOKIE_NAME, user.GetCookieString(hashValue));
            return cookie;
        }
    }

    public class OpenIdIdentity : IIdentity
    {
        private readonly OpenIdUser _user;
        public OpenIdIdentity(OpenIdUser user)
        {
            _user = user;
        }
        public OpenIdUser OpenIdUser
        {
            get
            {
                return _user;
            }
        }
        public string AuthenticationType
        {
            get
            {
                return "OpenID Identity";
            }
        }
        public bool IsAuthenticated
        {
            get
            {
                return true;
            }
        }
        public string Name
        {
            get
            {
                return _user.FullName ?? string.Empty;
            }
        }
    }

    public class OpenIdAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (
                HttpContext.Current.Session["User"] == null
                )
            {
                var authenticatedCookie = httpContext.Request.Cookies[OpenIdMembershipService.LOGIN_COOKIE_NAME];
                if (authenticatedCookie != null)
                {
                    var authenticatedCookieValue = authenticatedCookie.Value.ToString();
                    if (!string.IsNullOrWhiteSpace(authenticatedCookieValue))
                    {
                        var user = OpenIdUser.FromCookieString(authenticatedCookieValue);

                        HttpContext.Current.Session.Add("User", user);
                    }
                }
            }
            
            return HttpContext.Current.Session["User"] != null;
        }
    }

    public static class Extensions
    {
        public static bool IsSuccessful(this IAuthenticationResponse response)
        {
            return response != null && response.Status == AuthenticationStatus.Authenticated;
        }
    }
}