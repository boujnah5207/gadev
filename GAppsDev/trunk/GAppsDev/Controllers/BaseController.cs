using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using DA;
using GAppsDev.Models.ErrorModels;
using GAppsDev.OpenIdService;

namespace GAppsDev.Controllers
{
    public abstract class BaseController : Controller
    {
        
        public OpenIdUser CurrentUser 
        {
            get
            {
                return (OpenIdUser)Session["User"];
            }
        }

        public bool Authorized(params RoleType[] roles)
        {
            OpenIdUser user = CurrentUser;

            foreach (RoleType role in roles)
            {
                if (!Roles.HasRole(user.Roles, role))
                    return false;
            }

            return true;
        }

        public ActionResult Error(string errorMessage)
        {
            return View("Error", new ErrorModel(errorMessage));
        }
    }
}
