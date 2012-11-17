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
using GAppsDev.Models.SupplierModels;
using Mvc4.OpenId.Sample.Security;
using System.Globalization;
using GAppsDev.OpenIdService;

namespace GAppsDev.Controllers
{
    public class SuppliersController : BaseController
    {
        const int ITEMS_PER_PAGE = 10;
        const int FIRST_PAGE = 1;
        const string NO_SORT_BY = "None";
        const string DEFAULT_DESC_ORDER = "DESC";

        private Entities db = new Entities();

        //
        // GET: /Suppliers/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            return View(db.Suppliers.Where(x => x.CompanyId == CurrentUser.CompanyId).ToList());
        }


        [OpenIdAuthorize]
        public ActionResult Import()
        {
            return View(ViewBag.CompanyId = CurrentUser.CompanyId);
        }

        //
        // GET: /Suppliers/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            Supplier supplier = db.Suppliers.Single(s => s.Id == id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
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
                return Error(Errors.NO_PERMISSION);
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
                        return Error(Errors.SUPPLIERS_CREATE_ERROR);
                }
                else
                {
                    return Error(ModelState);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
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
                return Error(Errors.NO_PERMISSION);
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
                    return Json(new { success = false, message = Errors.SUPPLIERS_CREATE_ERROR }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = Errors.NO_PERMISSION }, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // GET: /Suppliers/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
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
            if (ModelState.IsValid)
            {
                db.Suppliers.Attach(supplier);
                db.ObjectStateManager.ChangeObjectState(supplier, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(supplier);
        }

        //
        // GET: /Suppliers/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
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
                                VAT_Number = supp.VAT_Number,
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
                    return Json(new { gotData = false, message = Errors.SUPPLIERS_GET_ERROR }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { gotData = false, message = Errors.NO_PERMISSION }, JsonRequestBehavior.AllowGet);
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
                    return Error(Errors.ORDER_GET_ERROR);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}