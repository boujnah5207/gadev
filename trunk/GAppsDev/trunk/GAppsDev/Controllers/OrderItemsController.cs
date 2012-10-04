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
using GAppsDev.Models.OrderItemsModels;

namespace GAppsDev.Controllers
{
    public class OrderItemsController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /OrderItems/

        public ActionResult Index()
        {
            var orders_items = db.Orders_Items.Include("Supplier");
            return View(orders_items.ToList());
        }

        //
        // GET: /OrderItems/Details/5

        public ActionResult Details(int id = 0)
        {
            Orders_Items orders_items = db.Orders_Items.Single(o => o.Id == id);
            if (orders_items == null)
            {
                return HttpNotFound();
            }
            return View(orders_items);
        }

        //
        // GET: /OrderItems/Create

        public ActionResult Create()
        {
            if (Authorized(RoleType.Employee))
            {
                List<Supplier> allSuppliers;
                using(SuppliersRepository suppliersRep = new SuppliersRepository())
                {
                    allSuppliers = suppliersRep.GetList().ToList();
                }

                if (allSuppliers != null)
                {
                    ViewBag.SupplierId = new SelectList(allSuppliers, "Id", "Name");
                    return View();
                }
                else
                {
                    return Error(Errors.SUPPLIERS_GET_ERROR);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /OrderItems/Create

        [HttpPost]
        public ActionResult Create(Orders_Items orders_items)
        {
            if (ModelState.IsValid)
            {
                db.Orders_Items.AddObject(orders_items);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Name", orders_items.SupplierId);
            return View(orders_items);
        }

        //
        // GET: /OrderItems/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Orders_Items orders_items = db.Orders_Items.Single(o => o.Id == id);
            if (orders_items == null)
            {
                return HttpNotFound();
            }
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Name", orders_items.SupplierId);
            return View(orders_items);
        }

        //
        // POST: /OrderItems/Edit/5

        [HttpPost]
        public ActionResult Edit(Orders_Items orders_items)
        {
            if (ModelState.IsValid)
            {
                db.Orders_Items.Attach(orders_items);
                db.ObjectStateManager.ChangeObjectState(orders_items, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Name", orders_items.SupplierId);
            return View(orders_items);
        }

        //
        // GET: /OrderItems/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Orders_Items orders_items = db.Orders_Items.Single(o => o.Id == id);
            if (orders_items == null)
            {
                return HttpNotFound();
            }
            return View(orders_items);
        }

        //
        // POST: /OrderItems/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Orders_Items orders_items = db.Orders_Items.Single(o => o.Id == id);
            db.Orders_Items.DeleteObject(orders_items);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public JsonResult GetBySupplier(int id)
        {
            if (Authorized(RoleType.Employee))
            {
                List<AjaxOrderItem> allItems;
                using (OrderItemsRepository itemRep = new OrderItemsRepository())
                {
                    allItems = itemRep.GetList().Where(item => item.SupplierId == id).Select(x => new AjaxOrderItem(){ Id = x.Id, Title = x.Title, SubTitle = x.SubTitle}).ToList();
                }

                if (allItems != null)
                {
                    return Json(new { gotData = true, data = allItems, message = String.Empty }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { gotData = false, message = Errors.ORDERITEMS_GET_ERROR }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { gotData = false, message = Errors.NO_PERMISSION }, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}