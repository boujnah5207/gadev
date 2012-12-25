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
            using (InventoryRepository inventoryRepository = new InventoryRepository(CurrentUser.CompanyId))
            {
                inventories = inventoryRepository.GetList("Orders_Items", "Location").Where(x => x.CompanyId == CurrentUser.CompanyId);

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
            using (OrderItemsRepository orderItemsRepository = new OrderItemsRepository(CurrentUser.CompanyId))
            using (SuppliersRepository suppliersRepository = new SuppliersRepository(CurrentUser.CompanyId))
            using (LocationsRepository locationsRepository = new LocationsRepository(CurrentUser.CompanyId))
            using (InventoryRepository inventoryRepository = new InventoryRepository(CurrentUser.CompanyId))
            {

                ViewBag.RelatedInventoryItem = new SelectList(inventoryRepository.GetList("Orders_Items")
                                  .Select( x => new { Id = x.Id, InventarNumber = x.InventarNumber, Title = x.Orders_Items.Title, SubTitle = x.Orders_Items.SubTitle })
                                  .ToList()
                  .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.InventarNumber + " " + x.Title + " " + x.SubTitle })
                  .OrderBy(x => x.Name)
                  .ToList(), "Id", "Name");


                if (locationsRepository.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).Count() == 0)
                    return Error(Loc.Dic.error_no_location_exist);
                ViewBag.LocationId = new SelectList(locationsRepository.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).OrderBy(x => x.Name).ToList(), "Id", "Name");

                ViewBag.Suppliers = new SelectList(suppliersRepository.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).OrderBy(x=>x.Name).ToList(), "Id", "Name");

                
            }
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
                inventory.AddedBy = CurrentUser.UserId;
                inventory.RemainingQuantity = inventory.OriginalQuantity;
                inventory.CreationDate = DateTime.Now;

                using (InventoryRepository inventoryRepository = new InventoryRepository(CurrentUser.CompanyId))
                {
                    if(inventoryRepository.Create(inventory))
                        return RedirectToAction("Index");
                    return Error(Loc.Dic.Error_DatabaseError);
                }

            }

            //using (OrderItemsRepository orderItemsRepository = new OrderItemsRepository())
            using (LocationsRepository locationsRepository = new LocationsRepository(CurrentUser.CompanyId))
            //using (InventoryRepository inventoryRepository = new InventoryRepository())
            {
                //ViewBag.RelatedInventoryItem = new SelectList(orderItemsRepository.GetList(), "Id", "Title" + "SubTitle");
                ViewBag.LocationId = new SelectList(locationsRepository.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId), "Id", "Name");
            }
            return View(inventory);
        }

        
        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            Inventory inventory = db.Inventories.Single(i => i.Id == id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", inventory.CompanyId);
            ViewBag.RelatedInventoryItem = new SelectList(db.Inventories, "Id", "Orders_Items.Title", inventory.RelatedInventoryItem);
            ViewBag.ItemId = new SelectList(db.Orders_Items, "Id", "Title", inventory.ItemId);
            ViewBag.LocationId = new SelectList(db.Locations, "Id", "Name", inventory.LocationId);
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
                inventory.CompanyId = CurrentUser.CompanyId;
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