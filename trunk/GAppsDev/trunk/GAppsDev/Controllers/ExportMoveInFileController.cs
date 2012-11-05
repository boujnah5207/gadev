using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using DB;


namespace GAppsDev.Controllers
{
    public class ExportMoveInFileController : BaseController
    {
        //
        // GET: /ExportMoveInFile/

        public ActionResult Index()
        {
            using (OrdersRepository ordersRepository = new OrdersRepository())
            {
                List<Order> toExportOrders = ordersRepository
                    .GetList()
                    .Where(x => x.CompanyId == CurrentUser.CompanyId && x.StatusId == (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport)
                    .ToList();
                return View(toExportOrders);
            }
        }

    }
}
