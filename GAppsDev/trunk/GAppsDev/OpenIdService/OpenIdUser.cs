using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using DA;
using DB;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

namespace GAppsDev.OpenIdService
{
    public class OpenIdUser
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return FirstName + " " + LastName; } }
        public int Roles { get; set; }
        public int? OrdersApproverId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastLogInTime { get; set; }
        public DateTime LastActionTime { get; set; }
        public string LanguageCode { get; set; }
        public bool IsActive { get; set; }

        public bool IsSignedByProvider { get; set; }
        public string ClaimedIdentifier { get; set; }

        public OpenIdUser()
        {
        }
        public OpenIdUser(string data)
        {
            populateFromDelimitedString(data);
        }
        public OpenIdUser(FetchResponse claim, string identifier)
        {
            addClaimInfo(claim, identifier);
        }
        private void addClaimInfo(FetchResponse claim, string identifier)
        {
            Email = claim.GetAttributeValue(WellKnownAttributes.Contact.Email);
            FirstName = claim.GetAttributeValue(WellKnownAttributes.Name.First);
            LastName = claim.GetAttributeValue(WellKnownAttributes.Name.Last);

            IsSignedByProvider = false;
            ClaimedIdentifier = identifier;
        }
        private void populateFromDelimitedString(string data)
        {
            if (data.Contains(","))
            {
                int claimedId;
                int claimedHash;
                var stringParts = data.Split(',');
                if (stringParts.Length > 0) claimedId = int.Parse(stringParts[0]);
                if (stringParts.Length > 1) claimedHash = int.Parse(stringParts[1]);
                if (stringParts.Length > 2) ClaimedIdentifier = stringParts[2];
            }
        }
        public override string ToString()
        {
            return String.Format("{0},{1},{2},{3},{4}", UserId, Email, FirstName, LastName, ClaimedIdentifier);
        }

        public string GetCookieString(string hashValue)
        {
            return String.Format("{0},{1},{2};", UserId, hashValue, ClaimedIdentifier);
        }

        public static OpenIdUser FromCookieString(string cookieString)
        {
            if (cookieString.Contains(","))
            {
                int claimedId;
                string claimedIdString = String.Empty;
                string claimedHashValue = String.Empty;
                string claimedIdentifier = String.Empty;

                var stringParts = cookieString.Split(',');
                if (stringParts.Length > 0) claimedIdString = stringParts[0];
                if (stringParts.Length > 1) claimedHashValue = stringParts[1];
                if (stringParts.Length > 2) claimedIdentifier = stringParts[2];

                bool isValidId = int.TryParse(claimedIdString, out claimedId);

                if (isValidId && !String.IsNullOrWhiteSpace(claimedHashValue))
                {
                    using (CookiesRepository cookiesRep = new CookiesRepository())
                    using (UserRepository userRep = new UserRepository())
                    {
                        bool isCookieValid = cookiesRep.GetList().Any(x => x.UserId == claimedId && x.HashValue == claimedHashValue);

                        if (isCookieValid)
                        {
                            User loggingUser = userRep.GetEntity(claimedId);

                            if (loggingUser != null)
                            {
                                return new OpenIdUser()
                                {
                                    UserId = loggingUser.Id,
                                    CompanyId = loggingUser.CompanyId,
                                    CompanyName = loggingUser.Company.Name,
                                    Email = loggingUser.Email,
                                    FirstName = loggingUser.FirstName,
                                    LastName = loggingUser.LastName,
                                    Roles = loggingUser.Roles,
                                    CreationTime = loggingUser.CreationTime,
                                    LastLogInTime = loggingUser.LastLogInTime,
                                    IsSignedByProvider = false,
                                    ClaimedIdentifier = claimedIdentifier,
                                    OrdersApproverId = loggingUser.OrdersApproverId,
                                    IsActive = loggingUser.IsActive,
                                    LanguageCode = loggingUser.Language.Code
                                };
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}