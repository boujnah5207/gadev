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
    public class OrdersHistoryController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /OrdersHistory/
        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var orders_history = db.Orders_History.Include("Order").Include("Orders_History_Actions").Include("User");
            return View(orders_history.ToList());
        }

        //
        // GET: /OrdersHistory/Details/5
        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            Orders_History orders_history = db.Orders_History.Single(o => o.Id == id);
            if (orders_history == null)
            {
                return HttpNotFound();
            }
            return View(orders_history);
        }

        //
        // GET: /OrdersHistory/Create
        [OpenIdAuthorize]
        public ActionResult Create()
        {
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "Notes");
            ViewBag.OrderHistoryActionId = new SelectList(db.Orders_History_Actions, "Id", "Name");
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email");
            return View();
        }

        //
        // POST: /OrdersHistory/Create
        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Orders_History orders_history)
        {
            if (ModelState.IsValid)
            {
                db.Orders_History.AddObject(orders_history);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OrderId = new SelectList(db.Orders, "Id", "Notes", orders_history.OrderId);
            ViewBag.OrderHistoryActionId = new SelectList(db.Orders_History_Actions, "Id", "Name", orders_history.OrderHistoryActionId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", orders_history.UserId);
            return View(orders_history);
        }

        //
        // GET: /OrdersHistory/Edit/5
        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            Orders_History orders_history = db.Orders_History.Single(o => o.Id == id);
            if (orders_history == null)
            {
                return HttpNotFound();
            }
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "Notes", orders_history.OrderId);
            ViewBag.OrderHistoryActionId = new SelectList(db.Orders_History_Actions, "Id", "Name", orders_history.OrderHistoryActionId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", orders_history.UserId);
            return View(orders_history);
        }

        //
        // POST: /OrdersHistory/Edit/5
        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Orders_History orders_history)
        {
            if (ModelState.IsValid)
            {
                db.Orders_History.Attach(orders_history);
                db.ObjectStateManager.ChangeObjectState(orders_history, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "Notes", orders_history.OrderId);
            ViewBag.OrderHistoryActionId = new SelectList(db.Orders_History_Actions, "Id", "Name", orders_history.OrderHistoryActionId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", orders_history.UserId);
            return View(orders_history);
        }

        //
        // GET: /OrdersHistory/Delete/5
        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            Orders_History orders_history = db.Orders_History.Single(o => o.Id == id);
            if (orders_history == null)
            {
                return HttpNotFound();
            }
            return View(orders_history);
        }

        //
        // POST: /OrdersHistory/Delete/5
        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Orders_History orders_history = db.Orders_History.Single(o => o.Id == id);
            db.Orders_History.DeleteObject(orders_history);
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