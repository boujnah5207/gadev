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
            if (CurrentUser == null)
                return false;

            foreach (RoleType role in roles)
            {
                if (!Roles.HasRole(CurrentUser.Roles, role))
                    return false;
            }

            return true;
        }

        public ActionResult Error(string errorMessage)
        {
            return View("Error", new ErrorModel(errorMessage));
        }

        public ActionResult Error(ModelStateDictionary modelState)
        {
            return View("Error", new ErrorModel(GetErrorsFromModel(modelState)));
        }

        protected string GetErrorsFromModel(ModelStateDictionary modelState)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(Errors.INVALID_FORM);
            builder.AppendLine();

            foreach (var kvp in modelState)
            {
                var fieldName = kvp.Key;
                var value = kvp.Value;

                if (value.Errors.Count > 0)
                {
                    builder.AppendLine(String.Format("בשדה \"{0}\":", fieldName, value.Value.AttemptedValue));
                    foreach (var error in value.Errors)
                    {
                        builder.AppendLine(error.ErrorMessage);
                    }
                }
            }

            builder.AppendLine();
            builder.AppendLine("אנא חזור ונסה שנית.");

            return builder.ToString();
        }
    }
}
