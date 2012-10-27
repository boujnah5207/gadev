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
    public class InventoryController : Controller
    {
        private Entities db = new Entities();
        [OpenIdAuthorize]
        public ActionResult Home()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult SubMenu()
        {
            return PartialView();
        }

        //
        // GET: /Inventory/

        public ActionResult Index()
        {
            var inventories = db.Inventories.Include("Company").Include("Inventory2").Include("Orders_Items").Include("Location");
            return View(inventories.ToList());
        }

        //
        // GET: /Inventory/Details/5

        public ActionResult Details(int id = 0)
        {
            Inventory inventory = db.Inventories.Single(i => i.Id == id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View(inventory);
        }

        //
        // GET: /Inventory/Create

        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name");
            ViewBag.RelatedInventoryItem = new SelectList(db.Inventories, "Id", "OrderId");
            ViewBag.ItemId = new SelectList(db.Orders_Items, "Id", "Title");
            ViewBag.LocationId = new SelectList(db.Locations, "Id", "City");
            return View();
        }

        //
        // POST: /Inventory/Create

        [HttpPost]
        public ActionResult Create(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                db.Inventories.AddObject(inventory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", inventory.CompanyId);
            ViewBag.RelatedInventoryItem = new SelectList(db.Inventories, "Id", "OrderId", inventory.RelatedInventoryItem);
            ViewBag.ItemId = new SelectList(db.Orders_Items, "Id", "Title", inventory.ItemId);
            ViewBag.LocationId = new SelectList(db.Locations, "Id", "City", inventory.LocationId);
            return View(inventory);
        }

        //
        // GET: /Inventory/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Inventory inventory = db.Inventories.Single(i => i.Id == id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", inventory.CompanyId);
            ViewBag.RelatedInventoryItem = new SelectList(db.Inventories, "Id", "OrderId", inventory.RelatedInventoryItem);
            ViewBag.ItemId = new SelectList(db.Orders_Items, "Id", "Title", inventory.ItemId);
            ViewBag.LocationId = new SelectList(db.Locations, "Id", "City", inventory.LocationId);
            return View(inventory);
        }

        //
        // POST: /Inventory/Edit/5

        [HttpPost]
        public ActionResult Edit(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                db.Inventories.Attach(inventory);
                db.ObjectStateManager.ChangeObjectState(inventory, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", inventory.CompanyId);
            ViewBag.RelatedInventoryItem = new SelectList(db.Inventories, "Id", "OrderId", inventory.RelatedInventoryItem);
            ViewBag.ItemId = new SelectList(db.Orders_Items, "Id", "Title", inventory.ItemId);
            ViewBag.LocationId = new SelectList(db.Locations, "Id", "City", inventory.LocationId);
            return View(inventory);
        }

        //
        // GET: /Inventory/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Inventory inventory = db.Inventories.Single(i => i.Id == id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View(inventory);
        }

        //
        // POST: /Inventory/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Inventory inventory = db.Inventories.Single(i => i.Id == id);
            db.Inventories.DeleteObject(inventory);
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