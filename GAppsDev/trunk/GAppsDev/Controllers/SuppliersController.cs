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

namespace GAppsDev.Controllers
{
    public class SuppliersController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /Suppliers/

        public ActionResult Index()
        {
            return View(db.Suppliers.ToList());
        }

        //
        // GET: /Suppliers/Details/5

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

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Suppliers/Create

        [HttpPost]
        public ActionResult Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                db.Suppliers.AddObject(supplier);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(supplier);
        }

        //
        // GET: /Suppliers/Edit/5

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
        public ActionResult DeleteConfirmed(int id)
        {
            Supplier supplier = db.Suppliers.Single(s => s.Id == id);
            db.Suppliers.DeleteObject(supplier);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public JsonResult GetAll()
        {
            if (Authorized(RoleType.Employee))
            {
                List<Supplier> allSuppliers;
                using (SuppliersRepository suppRep = new SuppliersRepository())
                {
                    allSuppliers = suppRep.GetList().ToList();
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