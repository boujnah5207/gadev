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
    public class LocationsController : BaseController
    {
        const int ITEMS_PER_PAGE = 10;
        const int FIRST_PAGE = 1;
        const string NO_SORT_BY = "None";
        const string DEFAULT_SORT = "name";
        const string DEFAULT_DESC_ORDER = "DESC";

        private Entities db = new Entities();

        //
        // GET: /Locations/

        [OpenIdAuthorize]
        public ActionResult Index(int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            if (!Authorized(RoleType.InventoryManager))
                return Error(Loc.Dic.error_no_permission);

            IEnumerable<Location> locations;
            using (LocationsRepository locationsRep = new LocationsRepository())
            {
                locations = locationsRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId);

                locations = Pagination(locations, page, sortby, order);

                return View(locations.ToList());
            }
        }

        //
        // GET: /Locations/Details/5
        [OpenIdAuthorize]

        public ActionResult Details(int id = 0)
        {
            Location location = db.Locations.Single(l => l.Id == id);
            if (location == null)
            {
                return HttpNotFound();
            }
            return View(location);
        }

        //
        // GET: /Locations/Create
        [OpenIdAuthorize]

        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name");
            return View();
        }

        //
        // POST: /Locations/Create
        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Location location)
        {
            
            if (ModelState.IsValid)
            {
                location.CompanyId = CurrentUser.CompanyId;
                using (LocationsRepository locationsRepository = new LocationsRepository())
                {
                    locationsRepository.Create(location);
                }
                return RedirectToAction("Index");
            }

            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", location.CompanyId);
            return View(location);
        }

        //
        // GET: /Locations/Edit/5
        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            Location location = db.Locations.Single(l => l.Id == id);
            if (location == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", location.CompanyId);
            return View(location);
        }

        //
        // POST: /Locations/Edit/5
        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Location location)
        {
            if (ModelState.IsValid)
            {
                location.CompanyId = CurrentUser.CompanyId;
                db.Locations.Attach(location);
                db.ObjectStateManager.ChangeObjectState(location, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", location.CompanyId);
            return View(location);
        }

        //
        // GET: /Locations/Delete/5
        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            Location location = db.Locations.Single(l => l.Id == id);
            if (location == null)
            {
                return HttpNotFound();
            }
            return View(location);
        }

        //
        // POST: /Locations/Delete/5
        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Location location = db.Locations.Single(l => l.Id == id);
            db.Locations.DeleteObject(location);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [ChildActionOnly]
        public ActionResult List(IEnumerable<Location> locations, string baseUrl, bool isOrdered, bool isPaged, string sortby, string order, int currPage, int numberOfPages, bool isCheckBoxed = false, bool showUserName = true)
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

            return PartialView(locations);
        }

        private IEnumerable<Location> Pagination(IEnumerable<Location> locations, int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            int numberOfItems = locations.Count();
            int numberOfPages = numberOfItems / ITEMS_PER_PAGE;
            if (numberOfItems % ITEMS_PER_PAGE != 0)
                numberOfPages++;

            if (page <= 0)
                page = FIRST_PAGE;
            if (page > numberOfPages)
                page = numberOfPages;

            if (sortby != NO_SORT_BY)
            {
                Func<Func<Location, dynamic>, IEnumerable<Location>> orderFunction;

                if (order == DEFAULT_DESC_ORDER)
                    orderFunction = x => locations.OrderByDescending(x);
                else
                    orderFunction = x => locations.OrderBy(x);

                switch (sortby)
                {
                    case "city":
                        locations = orderFunction(x => x.City);
                        break;
                    case "address":
                        locations = orderFunction(x => x.Address);
                        break;
                    case "name":
                    default:
                        locations = orderFunction(x => x.Name);
                        break;
                }
            }

            locations = locations
                .Skip((page - 1) * ITEMS_PER_PAGE)
                .Take(ITEMS_PER_PAGE)
                .ToList();

            ViewBag.Sortby = sortby;
            ViewBag.Order = order;
            ViewBag.CurrPage = page;
            ViewBag.NumberOfPages = numberOfPages;

            return locations;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}