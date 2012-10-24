using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DB;

namespace GAppsDev.Controllers
{
    public class PermissionsController : Controller
    {
        private Entities db = new Entities();

        //
        // GET: /Permissions/

        public ActionResult Index()
        {
            return View(db.Budgets_Permissions.ToList());
        }

        //
        // GET: /Permissions/Details/5

        public ActionResult Details(int id = 0)
        {
            Budgets_Permissions budgets_permissions = db.Budgets_Permissions.Single(b => b.Id == id);
            if (budgets_permissions == null)
            {
                return HttpNotFound();
            }
            return View(budgets_permissions);
        }

        //
        // GET: /Permissions/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Permissions/Create

        [HttpPost]
        public ActionResult Create(Budgets_Permissions budgets_permissions)
        {
            if (ModelState.IsValid)
            {
                db.Budgets_Permissions.AddObject(budgets_permissions);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(budgets_permissions);
        }

        //
        // GET: /Permissions/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Budgets_Permissions budgets_permissions = db.Budgets_Permissions.Single(b => b.Id == id);
            if (budgets_permissions == null)
            {
                return HttpNotFound();
            }
            return View(budgets_permissions);
        }

        //
        // POST: /Permissions/Edit/5

        [HttpPost]
        public ActionResult Edit(Budgets_Permissions budgets_permissions)
        {
            if (ModelState.IsValid)
            {
                db.Budgets_Permissions.Attach(budgets_permissions);
                db.ObjectStateManager.ChangeObjectState(budgets_permissions, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(budgets_permissions);
        }

        //
        // GET: /Permissions/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Budgets_Permissions budgets_permissions = db.Budgets_Permissions.Single(b => b.Id == id);
            if (budgets_permissions == null)
            {
                return HttpNotFound();
            }
            return View(budgets_permissions);
        }

        //
        // POST: /Permissions/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Budgets_Permissions budgets_permissions = db.Budgets_Permissions.Single(b => b.Id == id);
            db.Budgets_Permissions.DeleteObject(budgets_permissions);
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