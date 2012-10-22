using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GAppsDev.OpenIdService
{
    public class LogInResult
    {
        public bool IsAuthenticated { get; set; }

        public bool IsNewUser { get; set; }
        public bool IsRegistered { get; set; }
        public bool IsCanceled { get; set; }
        
        public OpenIdUser User { get; set; }

        public LogInResult()
        {
            IsAuthenticated = false;
            IsNewUser = false;
            IsRegistered = false;
            IsCanceled = false;
            User = null;
        }
    }
}