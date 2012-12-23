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
        public const string DEFAULT_SORT = "lastChange";

        [ChildActionOnly]
        public ActionResult PartialOrdersHistory(List<Orders_History> ordersHistoryList, string baseUrl, bool isOrdered, bool isPaged, string sortby, string order, int currPage, int numberOfPages, bool isCheckBoxed = false, bool showUserName = true)
        {
            ViewBag.BaseUrl = baseUrl;
            ViewBag.IsOrdered = isOrdered;
            ViewBag.IsPaged = isPaged;
            ViewBag.Sortby = sortby;
            ViewBag.Order = order;
            ViewBag.CurrPage = currPage;
            ViewBag.NumberOfPages = numberOfPages;

            ViewBag.IsCheckBoxed = isCheckBoxed;
            ViewBag.ShowUserName = showUserName;

            ViewBag.UserRoles = CurrentUser.Roles;

            return PartialView(ordersHistoryList);
        }

        [ChildActionOnly]
        public ActionResult SimpleListOrdersHistory(List<Orders_History> ordersHistoryList, string baseUrl)
        {
            ViewBag.BaseUrl = baseUrl;
            ViewBag.IsOrdered = false;
            ViewBag.IsPaged = false;
            ViewBag.Sortby = DEFAULT_SORT;
            ViewBag.Order = DEFAULT_DESC_ORDER;
            ViewBag.CurrPage = 1;
            ViewBag.NumberOfPages = 0;

            ViewBag.IsCheckBoxed = false;

            return PartialView(ordersHistoryList);
        }
    }
}