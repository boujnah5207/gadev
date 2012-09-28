using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using DB;
using GAppsDev.Models.ErrorModels;
using GAppsDev.OpenIdService;
using Mvc4.OpenId.Sample.Security;

namespace GAppsDev.Controllers
{
    public class OrdersController : Controller
    {
        private const int WAITING_FOR_APPROVAL_STATUS = 1;
        private Entities db = new Entities();

        //
        // GET: /Orders/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            using (OrdersRepository ordersRep = new OrdersRepository())
            {
                return View(ordersRep.GetList("Company", "Orders_Items", "Orders_Statuses", "Supplier", "User").ToList());
            }
            //var orders = db.Orders.Include("Company").Include("Orders_Items").Include("Orders_Statuses").Include("Supplier").Include("User");
            //return View(orders.ToList());
        }

        //
        // GET: /Orders/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            Order order = db.Orders.Single(o => o.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        //
        // GET: /Orders/Create

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            using (OrderItemsRepository itemsRep = new OrderItemsRepository())
            using (OrderStatusesRepository statusRep = new OrderStatusesRepository())
            using (SuppliersRepository suppliersRep = new SuppliersRepository())
            {
                ViewBag.ItemId = new SelectList(itemsRep.GetList().ToList(), "Id", "Title");
                ViewBag.StatusId = new SelectList(statusRep.GetList().ToList(), "Id", "Name");
                ViewBag.SupplierId = new SelectList(suppliersRep.GetList().ToList(), "Id", "Name");
            }
            
            return View();
        }

        //
        // POST: /Orders/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Order order)
        {
            if (ModelState.IsValid)
            {
                OpenIdUser currentUser = (OpenIdUser)Session["User"]; //((OpenIdIdentity)User.Identity).OpenIdUser;
                if (Roles.HasRole(currentUser.Roles, RoleType.Employee))
                {
                    order.UserId = currentUser.UserId;
                    order.CompanyId = currentUser.CompanyId;
                    order.CreationDate = DateTime.Now;
                    order.StatusId = WAITING_FOR_APPROVAL_STATUS;
                    order.OrderApproverNotes = String.Empty;

                    bool wasCreated;
                    using (OrdersRepository orderRep = new OrdersRepository())
                    {
                        wasCreated = orderRep.Create(order);
                    }

                    if (wasCreated)
                        return RedirectToAction("Index");
                    else
                        return View("Error", new ErrorModel("קרתה שגיאה בזמן השמירה למסד הנתונים. ההזמנה לא נשמרה במערכת."));
                }
                else
                {
                    return View("Error", new ErrorModel("אין לך הרשאה ליצור הזמנות. אנא פנה למנהל המערכת."));
                }
            }

            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", order.CompanyId);
            ViewBag.ItemId = new SelectList(db.Orders_Items, "Id", "Title", order.ItemId);
            ViewBag.StatusId = new SelectList(db.Orders_Statuses, "Id", "Name", order.StatusId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Name", order.SupplierId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", order.UserId);
            return View(order);
        }

        //
        // GET: /Orders/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            Order order = db.Orders.Single(o => o.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", order.CompanyId);
            ViewBag.ItemId = new SelectList(db.Orders_Items, "Id", "Title", order.ItemId);
            ViewBag.StatusId = new SelectList(db.Orders_Statuses, "Id", "Name", order.StatusId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Name", order.SupplierId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", order.UserId);
            return View(order);
        }

        //
        // POST: /Orders/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Attach(order);
                db.ObjectStateManager.ChangeObjectState(order, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", order.CompanyId);
            ViewBag.ItemId = new SelectList(db.Orders_Items, "Id", "Title", order.ItemId);
            ViewBag.StatusId = new SelectList(db.Orders_Statuses, "Id", "Name", order.StatusId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Name", order.SupplierId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", order.UserId);
            return View(order);
        }

        //
        // GET: /Orders/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            Order order = db.Orders.Single(o => o.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        //
        // POST: /Orders/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Single(o => o.Id == id);
            db.Orders.DeleteObject(order);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}