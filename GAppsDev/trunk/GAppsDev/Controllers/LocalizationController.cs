using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace GAppsDev.Controllers
{
    public class LocalizationController : Controller
    {
        //
        // GET: /Localization/

        public JavaScriptResult LocalizeJavascript(string get = null)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("var local = {};");

            if (get != null)
            {
                string[] terms = get.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in terms)
                {
                    builder.AppendLine(String.Format("var local.{0} = '{1}';", item, Loc.Dic.ResourceManager.GetString(item)));
                }
            }
            else
            {
                foreach (DictionaryEntry item in Loc.Dic.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true))
                {
                    builder.AppendLine(String.Format("local.{0} = '{1}';", item.Key, item.Value));
                }
            }

            return JavaScript(builder.ToString());
        }

    }
}
