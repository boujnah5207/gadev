using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using DA;
using System.Data.Objects;

namespace System.Web.Mvc.Html
{
    public static partial class LinkExtensions
    {
        public static MvcHtmlString GetMonthName(this HtmlHelper htmlHelper, int month)
        {
            switch (month)
            {
                case 1:
                    return new MvcHtmlString(Loc.Dic.January);
                case 2:
                    return new MvcHtmlString(Loc.Dic.February);
                case 3:
                    return new MvcHtmlString(Loc.Dic.March);
                case 4:
                    return new MvcHtmlString(Loc.Dic.April);
                case 5:
                    return new MvcHtmlString(Loc.Dic.May);
                case 6:
                    return new MvcHtmlString(Loc.Dic.June);
                case 7:
                    return new MvcHtmlString(Loc.Dic.July);
                case 8:
                    return new MvcHtmlString(Loc.Dic.August);
                case 9:
                    return new MvcHtmlString(Loc.Dic.September);
                case 10:
                    return new MvcHtmlString(Loc.Dic.October);
                case 11:
                    return new MvcHtmlString(Loc.Dic.November);
                case 12:
                    return new MvcHtmlString(Loc.Dic.December);
                default:
                    return new MvcHtmlString("Invalid Month Number");
            }
        }

        public static MvcHtmlString FilterLink(this HtmlHelper htmlHelper, string baseURL, string linkText, string linkSortby, string linkOrder, string currSortby)
        {
            string currSort = HttpContext.Current.Request.QueryString["sortby"];
            string currOrder = HttpContext.Current.Request.QueryString["order"];
            bool isCurrentSort = (currSort != "none") && (currSort == linkSortby);
            string indicator;

            if (currOrder != "ASC")
            {
                currOrder = "DESC";
            }

            if (isCurrentSort)
                indicator = "<div class='" + currOrder + "'></div>";
            else
                indicator = "";

            string URL = String.Format("<a href='{0}?sortby={1}&amp;order={2}'>{3}{4}</a>", baseURL, linkSortby, linkOrder, linkText, indicator);

            return new MvcHtmlString(URL);
        }

        public static MvcHtmlString Pagination(this HtmlHelper htmlHelper, string baseURL, int totalPages, int maxPageLinks, int currPage, string sortby, string order)
        {
            StringBuilder builder = new StringBuilder();

            if (totalPages > 0)
            {
                int prevPage = currPage > 1 ? currPage - 1 : 1;
                int nextPage = currPage + 1 <= totalPages ? currPage + 1 : totalPages;

                builder.AppendLine("<div class='Pagination'>");

                if (currPage != 1)
                    builder.AppendLine(GetPageLink(" << הקודם ", baseURL, prevPage, sortby, order));
                else
                    builder.AppendLine("<span class='currentPage'> << הקודם </span>");

                builder.AppendLine("<ul class='PageList'>");

                if (totalPages <= maxPageLinks)
                {
                    for (int page = 1; page <= totalPages; page++)
                    {
                        builder.AppendLine(GetPageListLink(page.ToString(), baseURL, page, currPage, sortby, order));
                    }
                }
                else
                {
                    if (currPage < totalPages - 10)
                    {
                        int startPage = currPage - 5 >= 1 ? currPage - 5 : 1;
                        int endPage = currPage + 5 <= totalPages ? currPage + 5 : totalPages;

                        for (int page = startPage; page <= endPage; page++)
                        {
                            builder.AppendLine(GetPageListLink(page.ToString(), baseURL, page, currPage, sortby, order));
                        }
                    }
                    /*
                    for (int page = totalPages - 5; page <= totalPages; page++)
                    {
                        builder.AppendLine(GetPageListLink(page.ToString(), baseURL, page, currPage, sortby, order));
                    }
                    */
                }

                builder.AppendLine("</ul>");

                if (currPage != totalPages)
                    builder.AppendLine(GetPageLink(" הבא >> ", baseURL, nextPage, sortby, order));
                else
                    builder.AppendLine("<span class='currentPage'> הבא >> </span>");

                builder.AppendLine("</div>");
            }

            return new MvcHtmlString(builder.ToString());
        }

        private static string GetPageLink(string linkText, string baseURL, int page, string sortby, string order)
        {
            return String.Format("<a href='{0}?page={1}&sortby={2}&order={3}' >{4}</a>", baseURL, page, sortby, order, linkText);
        }

        private static string GetPageListLink(string linkText, string baseURL, int page, int currPage, string sortby, string order)
        {
            if (page != currPage)
                return String.Format("<li><a href='{0}?page={1}&sortby={2}&order={3}' > {4} </a></li>", baseURL, page, sortby, order, linkText);
            else
                return String.Format("<span class='currentPage'> {0} </span>", linkText);
        }

        public static int CountUserOrders(this HtmlHelper htmlHelper, int userId, int companyId)
        {
            using (OrdersRepository ordersRep = new OrdersRepository(companyId))
            {
                return ordersRep.GetList().Where(x => x.UserId == userId).Count();
            }
        }

        public static int CountUserPendingOrders(this HtmlHelper htmlHelper, int userId, int companyId)
        {
            using (OrdersRepository ordersRep = new OrdersRepository(companyId))
            {
                return ordersRep.GetList().Where(x => x.NextOrderApproverId == userId && x.StatusId != (int)StatusType.PendingOrderCreator).Count();
            }
        }

        public static int CountDelayingOrders(this HtmlHelper htmlHelper, int companyId)
        {
            using (OrdersRepository ordersRep = new OrdersRepository(companyId))
            {
                DateTime CurrentTime = DateTime.Now;
                return ordersRep.GetList("Orders_Statuses", "Supplier", "User")
                    .Where(x =>
                        EntityFunctions.DiffHours(x.CreationDate, CurrentTime) >= 48 &&
                        x.StatusId < (int)StatusType.ApprovedPendingInvoice &&
                        x.StatusId != (int)StatusType.Declined &&
                        x.StatusId != (int)StatusType.OrderCancelled
                        )
                    .Count();
            }
        }

        public static int CountPendingExport(this HtmlHelper htmlHelper, int companyId)
        {
            using (OrdersRepository ordersRep = new OrdersRepository(companyId))
            {
                return ordersRep.GetList("Orders_Statuses", "Supplier", "User")
                    .Where(x => x.StatusId == (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport)
                    .Count();
            }
        }

        public static int CountPendingInventory(this HtmlHelper htmlHelper, int companyId)
        {
            using (OrdersRepository ordersRep = new OrdersRepository(companyId))
            {
                return ordersRep.GetList("Orders_Statuses", "Supplier", "User")
                    .Where(x =>
                        !x.WasAddedToInventory &&
                        x.StatusId > (int)StatusType.InvoiceScannedPendingOrderCreator)
                    .Count();
            }
        }

        public static MvcHtmlString DisplayDecimal(decimal? value, int precision)
        {
            if(!value.HasValue) return new MvcHtmlString("0");

            int multiply = 1;
            string zeroFormat = "0.";
            for (int i = 0; i < precision; i++)
            {
                multiply *= 10;
                zeroFormat += "#";
            }

            value = Math.Floor(value.Value * multiply) / multiply;

            return new MvcHtmlString(value.Value.ToString(zeroFormat));
        }
    }
}
