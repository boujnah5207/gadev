using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mvc4.OpenId.Sample.Security;
using DB;
using DA;

namespace GAppsDev.Controllers
{
    public class HomeController : Controller
    {
        [OpenIdAuthorize]
        public ActionResult Index()
        {
            return View();
        }

        [OpenIdAuthorize]
        public ActionResult About()
        {
            return View();
        }

        [OpenIdAuthorize]
        public ActionResult Contact()
        {
            return View();
        }
    }
}
