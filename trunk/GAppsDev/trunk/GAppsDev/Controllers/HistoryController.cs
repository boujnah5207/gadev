using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DB;
using Mvc4.OpenId.Sample.Security;

namespace GAppsDev.Controllers
{
    public class HistoryController : BaseController
    {
        [ChildActionOnly]
        public ActionResult PartialOrdersHistory(List<Orders_History> ordersHistoryList)
        {
            return View(ordersHistoryList);
        }

    }
}