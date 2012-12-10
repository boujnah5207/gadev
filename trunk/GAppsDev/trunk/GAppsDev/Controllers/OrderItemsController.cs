using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using DB;
using GAppsDev.Models.OrderItemsModels;
using Mvc4.OpenId.Sample.Security;

namespace GAppsDev.Controllers
{
    public class OrderItemsController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /OrderItems/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var orders_items = db.Orders_Items.Include("Supplier");
            return View(orders_items.ToList());
        }

        //
        // GET: /OrderItems/Details/5

        [OpenIdAuthorize]
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

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            if (Authorized(RoleType.OrdersWriter))
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
                    return Error(Loc.Dic.error_suppliers_get_error);
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // POST: /OrderItems/Create

        [HttpPost]
        [OpenIdAuthorize]
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

        [OpenIdAuthorize]
        public ActionResult PopOutCreate()
        {
            if (Authorized(RoleType.OrdersWriter))
            {
                return PartialView();
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        public JsonResult AjaxCreate(Orders_Items orderItem)
        {
            if (Authorized(RoleType.OrdersWriter))
            {
                bool wasCreated;
                using (OrderItemsRepository itemRep = new OrderItemsRepository(CurrentUser.CompanyId))
                {
                    wasCreated = itemRep.Create(orderItem);
                }

                if (wasCreated)
                    return Json(new { success = true, message = String.Empty, newItemId = orderItem.Id }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = Loc.Dic.error_supplier_item_create_error }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = Loc.Dic.error_no_permission }, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // GET: /OrderItems/Edit/5

        [OpenIdAuthorize]
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
        [OpenIdAuthorize]
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

        [OpenIdAuthorize]
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
        [OpenIdAuthorize]
        public ActionResult DeleteConfirmed(int id)
        {
            Orders_Items orders_items = db.Orders_Items.Single(o => o.Id == id);
            db.Orders_Items.DeleteObject(orders_items);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public JsonResult GetBySupplier(int? id = null)
        {
            if (Authorized(RoleType.OrdersWriter))
            {
                List<AjaxOrderItem> allItems;
                using (OrderItemsRepository itemRep = new OrderItemsRepository(CurrentUser.CompanyId))
                {
                    if(id.HasValue)
                        allItems = itemRep.GetList().Where(item => item.SupplierId == id && item.CompanyId == CurrentUser.CompanyId).Select(x => new AjaxOrderItem() { Id = x.Id, Title = x.Title, SubTitle = x.SubTitle }).ToList();
                    else
                        allItems = itemRep.GetList().Where(item => item.SupplierId == null && item.CompanyId == CurrentUser.CompanyId).Select(x => new AjaxOrderItem() { Id = x.Id, Title = x.Title, SubTitle = x.SubTitle }).ToList();

                }

                if (allItems != null)
                {
                    return Json(new { gotData = true, data = allItems, message = String.Empty }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { gotData = false, message = Loc.Dic.error_orderitems_get_error }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { gotData = false, message = Loc.Dic.error_no_permission }, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}