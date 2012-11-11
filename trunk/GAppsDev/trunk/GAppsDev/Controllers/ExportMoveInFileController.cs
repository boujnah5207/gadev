using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using DB;
using Mvc4.OpenId.Sample.Security;


namespace GAppsDev.Controllers
{
    [OpenIdAuthorize]
    public class ExportMoveInFileController : BaseController
    {
        //
        // GET: /ExportMoveInFile/

        public ActionResult Index()
        {
            using (OrdersRepository ordersRepository = new OrdersRepository(CurrentUser.CompanyId))
            {
                List<Order> toExportOrders = ordersRepository
                    .GetList("Orders_Statuses", "Supplier", "User")
                    .Where(x => x.CompanyId == CurrentUser.CompanyId && x.StatusId == (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport)
                    .ToList();
                return View(toExportOrders);
            }
        }

    }
}
