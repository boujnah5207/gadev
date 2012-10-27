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
    public class DepartmentsController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /Departments/
        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var departments = db.Departments.Include("Company").Where(x => x.CompanyId == CurrentUser.CompanyId);
            return View(departments.ToList());
        }

        //
        // GET: /Departments/Details/5

        public ActionResult Details(int id = 0)
        {
            Department department = db.Departments.Single(d => d.Id == id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        //
        // GET: /Departments/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Departments/Create

        [HttpPost]
        public ActionResult Create(Department department)
        {

            if (ModelState.IsValid)
            {
                using (DepartmentsRepository DepartmentsRepository = new DepartmentsRepository())
                {
                    department.CompanyId = CurrentUser.CompanyId;
                    db.Departments.AddObject(department);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            return View(department);
        }

        //
        // GET: /Departments/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Department department = db.Departments.Single(d => d.Id == id);
            if (department == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", department.CompanyId);
            return View(department);
        }

        //
        // POST: /Departments/Edit/5

        [HttpPost]
        public ActionResult Edit(Department department)
        {
            if (ModelState.IsValid)
            {
                db.Departments.Attach(department);
                db.ObjectStateManager.ChangeObjectState(department, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", department.CompanyId);
            return View(department);
        }

        //
        // GET: /Departments/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Department department = db.Departments.Single(d => d.Id == id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        //
        // POST: /Departments/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Department department = db.Departments.Single(d => d.Id == id);
            db.Departments.DeleteObject(department);
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