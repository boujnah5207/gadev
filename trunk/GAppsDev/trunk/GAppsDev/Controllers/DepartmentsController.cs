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
    public class DepartmentsController : Controller
    {
        private Entities db = new Entities();

        //
        // GET: /Departments/

        public ActionResult Index()
        {
            var budget_departments = db.Budget_Departments.Include("Company");
            return View(budget_departments.ToList());
        }

        //
        // GET: /Departments/Details/5

        public ActionResult Details(int id = 0)
        {
            Budget_Departments budget_departments = db.Budget_Departments.Single(b => b.Id == id);
            if (budget_departments == null)
            {
                return HttpNotFound();
            }
            return View(budget_departments);
        }

        //
        // GET: /Departments/Create

        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name");
            return View();
        }

        //
        // POST: /Departments/Create

        [HttpPost]
        public ActionResult Create(Budget_Departments budget_departments)
        {
            if (ModelState.IsValid)
            {
                db.Budget_Departments.AddObject(budget_departments);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", budget_departments.CompanyId);
            return View(budget_departments);
        }

        //
        // GET: /Departments/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Budget_Departments budget_departments = db.Budget_Departments.Single(b => b.Id == id);
            if (budget_departments == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", budget_departments.CompanyId);
            return View(budget_departments);
        }

        //
        // POST: /Departments/Edit/5

        [HttpPost]
        public ActionResult Edit(Budget_Departments budget_departments)
        {
            if (ModelState.IsValid)
            {
                db.Budget_Departments.Attach(budget_departments);
                db.ObjectStateManager.ChangeObjectState(budget_departments, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", budget_departments.CompanyId);
            return View(budget_departments);
        }

        //
        // GET: /Departments/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Budget_Departments budget_departments = db.Budget_Departments.Single(b => b.Id == id);
            if (budget_departments == null)
            {
                return HttpNotFound();
            }
            return View(budget_departments);
        }

        //
        // POST: /Departments/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Budget_Departments budget_departments = db.Budget_Departments.Single(b => b.Id == id);
            db.Budget_Departments.DeleteObject(budget_departments);
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