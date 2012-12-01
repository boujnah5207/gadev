using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using DB;
using GAppsDev.Models.SupplierModels;
using Mvc4.OpenId.Sample.Security;
using System.Globalization;
using GAppsDev.OpenIdService;
using BL;

namespace GAppsDev.Controllers
{
    public class SuppliersController : BaseController
    {
        const int ITEMS_PER_PAGE = 10;
        const int FIRST_PAGE = 1;
        const string NO_SORT_BY = "None";
        const string DEFAULT_SORT = "name";
        const string DEFAULT_DESC_ORDER = "DESC";

        private Entities db = new Entities();

        //
        // GET: /Suppliers/

        [OpenIdAuthorize]
        public ActionResult Index(int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            if(!Authorized(RoleType.SystemManager))
                return Error(Loc.Dic.error_no_permission);

            IEnumerable<Supplier> suppliers;
            using(SuppliersRepository suppliersRep = new SuppliersRepository())
            {
                suppliers = suppliersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId);

                suppliers = Pagination(suppliers, page, sortby, order);

                return View(suppliers.ToList());
            }
        }

        [OpenIdAuthorize]
        public ActionResult Import()
        {
            if (Authorized(RoleType.SystemManager))
            {
                return View();
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Import(HttpPostedFileBase file)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);
            if (file != null && file.ContentLength <= 0) return Error(Loc.Dic.error_invalid_form);
            string moved = Interfaces.ImportSuppliers(file.InputStream, CurrentUser.CompanyId);
            if (moved == "OK") return RedirectToAction("index");
            else return Error(moved);
        }


        //
        // GET: /Suppliers/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            Supplier supplier;
            using (SuppliersRepository supplierssRep = new SuppliersRepository())
            {
                supplier = supplierssRep.GetEntity(id);
            }

            if (supplier == null)
                return Error(Loc.Dic.error_supplier_not_found);

            return View(supplier);
        }

        [ChildActionOnly]
        [OpenIdAuthorize]
        public ActionResult PartialDetails(Supplier supplier)
        {
            return PartialView(supplier);
        }

        //
        // GET: /Suppliers/Create

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            if (Authorized(RoleType.OrdersWriter))
            {
                return View();
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // POST: /Suppliers/Create

        [HttpPost]
        [OpenIdAuthorize]
        public ActionResult Create(Supplier supplier)
        {
            if (Authorized(RoleType.OrdersWriter))
            {
                if (ModelState.IsValid)
                {
                    bool wasCreated;
                    using (SuppliersRepository supplierRep = new SuppliersRepository())
                    {
                        supplier.CompanyId = CurrentUser.CompanyId;
                        wasCreated = supplierRep.Create(supplier);
                    }

                    if (wasCreated)
                        return RedirectToAction("Index");
                    else
                        return Error(Loc.Dic.error_suppliers_create_error);
                }
                else
                {
                    return Error(ModelState);
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
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

        public JsonResult AjaxCreate(Supplier supplier)
        {
            if (Authorized(RoleType.OrdersWriter))
            {
                supplier.CompanyId = CurrentUser.CompanyId;
                bool wasCreated;
                using (SuppliersRepository supplierRep = new SuppliersRepository())
                {
                    wasCreated = supplierRep.Create(supplier);
                }

                if (wasCreated)
                    return Json(new { success = true, message = String.Empty, newSupplierId = supplier.Id.ToString() }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = Loc.Dic.error_suppliers_create_error }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = Loc.Dic.error_no_permission }, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // GET: /Suppliers/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            if (!Authorized(RoleType.SystemManager))
                return Error(Loc.Dic.error_no_permission);

            Supplier supplier = db.Suppliers.Single(s => s.Id == id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }

        //
        // POST: /Suppliers/Edit/5

        [HttpPost]
        [OpenIdAuthorize]
        public ActionResult Edit(Supplier supplier)
        {
            if (!Authorized(RoleType.SystemManager))
                return Error(Loc.Dic.error_no_permission);

            if (ModelState.IsValid)
            {
                Supplier supplierFromDB;
                bool wasUpdated;
                using (SuppliersRepository supplierRep = new SuppliersRepository())
                {
                    supplierFromDB = supplierRep.GetEntity(supplier.Id);

                    if (supplierFromDB == null)
                        return Error(Loc.Dic.error_supplier_not_found);

                    if (supplierFromDB.CompanyId != CurrentUser.CompanyId)
                        return Error(Loc.Dic.error_no_permission);

                    supplierFromDB.Name = supplier.Name;
                    supplierFromDB.ExternalId = supplier.ExternalId;
                    supplierFromDB.Activity_Hours = supplier.Activity_Hours;
                    supplierFromDB.Additional_Phone = supplier.Additional_Phone;
                    supplierFromDB.Address = supplier.Address;
                    supplierFromDB.Branch_line = supplier.Branch_line;
                    supplierFromDB.Crew_Number = supplier.Crew_Number;
                    supplierFromDB.Customer_Number = supplier.Customer_Number;
                    supplierFromDB.EMail = supplier.EMail;
                    supplierFromDB.Fax = supplier.Fax;
                    supplierFromDB.Notes = supplier.Notes;
                    supplierFromDB.Phone_Number = supplier.Phone_Number;
                    supplierFromDB.Presentor_name = supplier.Presentor_name;
                    supplierFromDB.VAT_Number = supplier.VAT_Number;

                    if (supplierRep.Update(supplierFromDB) != null)
                        return RedirectToAction("Index");
                    else
                        return Error(Loc.Dic.error_database_error);
                }
            }
            else
            {
                return Error(ModelState);
            }
        }

        //
        // GET: /Suppliers/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            if (!Authorized(RoleType.SystemManager))
                return Error(Loc.Dic.error_no_permission);

            Supplier supplier = db.Suppliers.Single(s => s.Id == id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }

        //
        // POST: /Suppliers/Delete/5

        [HttpPost, ActionName("Delete")]
        [OpenIdAuthorize]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!Authorized(RoleType.SystemManager))
                return Error(Loc.Dic.error_no_permission);

            Supplier supplier = db.Suppliers.Single(s => s.Id == id);
            db.Suppliers.DeleteObject(supplier);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public JsonResult GetAll()
        {
            if (Authorized(RoleType.OrdersWriter))
            {
                List<AjaxSupplier> allSuppliers;
                using (SuppliersRepository suppRep = new SuppliersRepository())
                {
                    allSuppliers = suppRep.GetList()
                        .Where(x => x.CompanyId == CurrentUser.CompanyId)
                        .Select(
                            supp => new AjaxSupplier()
                            {
                                Id = supp.Id,
                                Name = supp.Name,
                                VAT_Number = supp.VAT_Number.HasValue ? supp.VAT_Number.Value : 0,
                                Phone_Number = supp.Phone_Number,
                                Activity_Hours = supp.Activity_Hours,
                                Additional_Phone = supp.Additional_Phone,
                                Address = supp.Address,
                                Branch_Line = supp.Branch_line,
                                City = supp.City,
                                Crew_Number = supp.Crew_Number,
                                Customer_Number = supp.Customer_Number,
                                EMail = supp.EMail,
                                Fax = supp.Fax,
                                Presentor_name = supp.Presentor_name,
                                Notes = supp.Notes,
                                CreationDate = supp.CreationDate
                            })
                        .ToList();
                }

                if (allSuppliers != null)
                {
                    return Json(new { gotData = true, data = allSuppliers, message = String.Empty }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { gotData = false, message = Loc.Dic.error_suppliers_get_error }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { gotData = false, message = Loc.Dic.error_no_permission }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SuppliersEntrence()
        {
            CultureInfo ci = new CultureInfo("He");
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            System.Threading.Thread.CurrentThread.CurrentCulture =
            CultureInfo.CreateSpecificCulture(ci.Name);

            return View();
        }

        [HttpPost]
        public ActionResult SuppliersEntrence(int page = FIRST_PAGE, string sortby = NO_SORT_BY, string order = DEFAULT_DESC_ORDER, int VAT_Number = 0)
        {
            CultureInfo ci = new CultureInfo("He");
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            System.Threading.Thread.CurrentThread.CurrentCulture =
            CultureInfo.CreateSpecificCulture(ci.Name);

            using (SuppliersRepository supRep = new SuppliersRepository())
            using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                Supplier supplier = supRep.GetList().SingleOrDefault(x => x.VAT_Number == VAT_Number);

                IEnumerable<Order> orders;

                orders = orderRep.GetList("Orders_Statuses", "Supplier", "User").Where(x => x.SupplierId == supplier.Id);

                if (orders != null)
                {
                    int numberOfItems = orders.Count();
                    int numberOfPages = numberOfItems / ITEMS_PER_PAGE;
                    if (numberOfItems % ITEMS_PER_PAGE != 0)
                        numberOfPages++;

                    if (page <= 0)
                        page = FIRST_PAGE;
                    if (page > numberOfPages)
                        page = numberOfPages;

                    if (sortby != NO_SORT_BY)
                    {
                        Func<Func<Order, dynamic>, IEnumerable<Order>> orderFunction;

                        if (order == DEFAULT_DESC_ORDER)
                            orderFunction = x => orders.OrderByDescending(x);
                        else
                            orderFunction = x => orders.OrderBy(x);

                        switch (sortby)
                        {
                            case "username":
                            default:
                                orders = orderFunction(x => x.User.FirstName + " " + x.User.LastName);
                                break;
                            case "number":
                                orders = orderFunction(x => x.OrderNumber);
                                break;
                            case "creation":
                                orders = orderFunction(x => x.CreationDate);
                                break;
                            case "supplier":
                                orders = orderFunction(x => x.Supplier.Name);
                                break;
                            case "status":
                                orders = orderFunction(x => x.StatusId);
                                break;
                            case "price":
                                orders = orderFunction(x => x.Price);
                                break;
                        }
                    }

                    orders = orders
                        .Skip((page - 1) * ITEMS_PER_PAGE)
                        .Take(ITEMS_PER_PAGE)
                        .ToList();

                    ViewBag.Sortby = sortby;
                    ViewBag.Order = order;
                    ViewBag.CurrPage = page;
                    ViewBag.NumberOfPages = numberOfPages;

                    return View(orders.ToList());
                }
                else
                {
                    return Error(Loc.Dic.error_order_get_error);
                }
            }
        }

        [ChildActionOnly]
        public ActionResult List(IEnumerable<Supplier> suppliers, string baseUrl, bool isOrdered, bool isPaged, string sortby, string order, int currPage, int numberOfPages, bool isCheckBoxed = false, bool showUserName = true)
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

            return PartialView(suppliers);
        }

        private IEnumerable<Supplier> Pagination(IEnumerable<Supplier> suppliers, int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            int numberOfItems = suppliers.Count();
            int numberOfPages = numberOfItems / ITEMS_PER_PAGE;
            if (numberOfItems % ITEMS_PER_PAGE != 0)
                numberOfPages++;

            if (page <= 0)
                page = FIRST_PAGE;
            if (page > numberOfPages)
                page = numberOfPages;

            if (sortby != NO_SORT_BY)
            {
                Func<Func<Supplier, dynamic>, IEnumerable<Supplier>> orderFunction;

                if (order == DEFAULT_DESC_ORDER)
                    orderFunction = x => suppliers.OrderByDescending(x);
                else
                    orderFunction = x => suppliers.OrderBy(x);

                switch (sortby)
                {
                    case "number":
                        suppliers = orderFunction(x => x.ExternalId);
                        break;
                    case "date":
                        suppliers = orderFunction(x => x.CreationDate);
                        break;
                    case "name":
                    default:
                        suppliers = orderFunction(x => x.Name);
                        break;
                }
            }

            suppliers = suppliers
                .Skip((page - 1) * ITEMS_PER_PAGE)
                .Take(ITEMS_PER_PAGE)
                .ToList();

            ViewBag.Sortby = sortby;
            ViewBag.Order = order;
            ViewBag.CurrPage = page;
            ViewBag.NumberOfPages = numberOfPages;

            return suppliers;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}