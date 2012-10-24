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

namespace GAppsDev.Controllers
{
    public class SuppliersController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /Suppliers/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            return View(db.Suppliers.ToList());
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
                    return Json(new { success = true, message = String.Empty }, JsonRequestBehavior.AllowGet);
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
                        .Where(x=>x.CompanyId==CurrentUser.CompanyId)
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

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}