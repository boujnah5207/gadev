using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DB;
using Mvc4.OpenId.Sample.Security;
using DA;
using GAppsDev.Models.InventoryItemModel;

namespace GAppsDev.Controllers
{
    public class InventoryController : BaseController
    {
        const int ITEMS_PER_PAGE = 10;
        const int FIRST_PAGE = 1;
        const string NO_SORT_BY = "None";
        const string DEFAULT_SORT = "name";
        const string DEFAULT_DESC_ORDER = "DESC";

        private Entities db = new Entities();

        [OpenIdAuthorize]
        public ActionResult Home()
        {
            return View();
        }

        //
        // GET: /Inventory/

        [OpenIdAuthorize]
        public ActionResult Index(int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            IEnumerable<Inventory> inventories;
            using (InventoryRepository inventoryRepository = new InventoryRepository())
            {
                inventories = inventoryRepository.GetList("Orders_Items","Location").Where(x => x.CompanyId == CurrentUser.CompanyId);

                inventories = Pagination(inventories, page, sortby, order);

                ViewBag.CurrentUser = CurrentUser;
                return View(inventories.ToList());
            }
        }

        //
        // GET: /Inventory/Details/5

        [OpenIdAuthorize]
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

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            Inventory inventory = new Inventory();
            Orders_Items item = new Orders_Items();
            ManualCreateInventoryItemModel manualCreateInventoryItemModel = new ManualCreateInventoryItemModel();
            inventory.CompanyId = CurrentUser.CompanyId;
            manualCreateInventoryItemModel.inventoryItem = inventory;
            manualCreateInventoryItemModel.item = item;

            ViewBag.RelatedInventoryItem = new SelectList(db.Inventories, "Id", "OrderId");
            ViewBag.LocationId = new SelectList(db.Locations.Where(x=>x.CompanyId == CurrentUser.CompanyId), "Id", "Name");
            return View();
        }

        //
        // POST: /Inventory/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                inventory.CompanyId = CurrentUser.CompanyId;
                db.Inventories.AddObject(inventory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", inventory.CompanyId);
            ViewBag.RelatedInventoryItem = new SelectList(db.Inventories, "Id", "OrderId", inventory.RelatedInventoryItem);
            ViewBag.ItemId = new SelectList(db.Orders_Items, "Id", "Title", inventory.ItemId);
            ViewBag.LocationId = new SelectList(db.Locations, "Id", "Name", inventory.LocationId);
            return View(inventory);
        }

        //
        // GET: /Inventory/Edit/5

        [OpenIdAuthorize]
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

        [OpenIdAuthorize]
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

        [OpenIdAuthorize]
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

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Inventory inventory = db.Inventories.Single(i => i.Id == id);
            db.Inventories.DeleteObject(inventory);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        [ChildActionOnly]
        public ActionResult List(IEnumerable<Inventory> inventoryItems, string baseUrl, bool isOrdered, bool isPaged, string sortby, string order, int currPage, int numberOfPages, bool isCheckBoxed = false, bool showUserName = true)
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

            return PartialView(inventoryItems);
        }

        private IEnumerable<Inventory> Pagination(IEnumerable<Inventory> inventoryItems, int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            int numberOfItems = inventoryItems.Count();
            int numberOfPages = numberOfItems / ITEMS_PER_PAGE;
            if (numberOfItems % ITEMS_PER_PAGE != 0)
                numberOfPages++;

            if (page <= 0)
                page = FIRST_PAGE;
            if (page > numberOfPages)
                page = numberOfPages;

            if (sortby != NO_SORT_BY)
            {
                Func<Func<Inventory, dynamic>, IEnumerable<Inventory>> orderFunction;

                if (order == DEFAULT_DESC_ORDER)
                    orderFunction = x => inventoryItems.OrderByDescending(x);
                else
                    orderFunction = x => inventoryItems.OrderBy(x);

                switch (sortby)
                {
                    case "subtitle":
                        inventoryItems = orderFunction(x => x.Orders_Items == null ? String.Empty : x.Orders_Items.Title);
                        break;
                    case "title":
                    default:
                        inventoryItems = orderFunction(x => x.Orders_Items == null ? String.Empty : x.Orders_Items.SubTitle);
                        break;
                }
            }

            inventoryItems = inventoryItems
                .Skip((page - 1) * ITEMS_PER_PAGE)
                .Take(ITEMS_PER_PAGE)
                .ToList();

            ViewBag.Sortby = sortby;
            ViewBag.Order = order;
            ViewBag.CurrPage = page;
            ViewBag.NumberOfPages = numberOfPages;

            return inventoryItems;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}