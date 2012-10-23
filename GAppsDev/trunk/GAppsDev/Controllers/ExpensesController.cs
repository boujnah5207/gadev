using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DB;
using Mvc4.OpenId.Sample.Security;

namespace GAppsDev.Controllers
{
    public class ExpensesController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /Expenses/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var budgets_expenses = db.Budgets_Expenses.Include("Department").Include("Projects_ParentProject").Include("Projects_SubProject");
            return View(budgets_expenses.ToList());
        }

        //
        // GET: /Expenses/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            Budgets_Expenses budgets_expenses = db.Budgets_Expenses.Single(b => b.Id == id);
            if (budgets_expenses == null)
            {
                return HttpNotFound();
            }
            return View(budgets_expenses);
        }

        //
        // GET: /Expenses/Create

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            ViewBag.DepartmentId = new SelectList(db.Departments, "Id", "Id");
            ViewBag.ParentProjectId = new SelectList(db.Projects_ParentProject, "Id", "Name");
            ViewBag.SubProjectId = new SelectList(db.Projects_SubProject, "Id", "Name");
            return View();
        }

        //
        // POST: /Expenses/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Budgets_Expenses budgets_expenses)
        {
            if (ModelState.IsValid)
            {
                db.Budgets_Expenses.AddObject(budgets_expenses);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentId = new SelectList(db.Departments, "Id", "Id", budgets_expenses.DepartmentId);
            ViewBag.ParentProjectId = new SelectList(db.Projects_ParentProject, "Id", "Name", budgets_expenses.ParentProjectId);
            ViewBag.SubProjectId = new SelectList(db.Projects_SubProject, "Id", "Name", budgets_expenses.SubProjectId);
            return View(budgets_expenses);
        }

        //
        // GET: /Expenses/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            Budgets_Expenses budgets_expenses = db.Budgets_Expenses.Single(b => b.Id == id);
            if (budgets_expenses == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(db.Departments, "Id", "Id", budgets_expenses.DepartmentId);
            ViewBag.ParentProjectId = new SelectList(db.Projects_ParentProject, "Id", "Name", budgets_expenses.ParentProjectId);
            ViewBag.SubProjectId = new SelectList(db.Projects_SubProject, "Id", "Name", budgets_expenses.SubProjectId);
            return View(budgets_expenses);
        }

        //
        // POST: /Expenses/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Budgets_Expenses budgets_expenses)
        {
            if (ModelState.IsValid)
            {
                db.Budgets_Expenses.Attach(budgets_expenses);
                db.ObjectStateManager.ChangeObjectState(budgets_expenses, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentId = new SelectList(db.Departments, "Id", "Id", budgets_expenses.DepartmentId);
            ViewBag.ParentProjectId = new SelectList(db.Projects_ParentProject, "Id", "Name", budgets_expenses.ParentProjectId);
            ViewBag.SubProjectId = new SelectList(db.Projects_SubProject, "Id", "Name", budgets_expenses.SubProjectId);
            return View(budgets_expenses);
        }

        //
        // GET: /Expenses/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            Budgets_Expenses budgets_expenses = db.Budgets_Expenses.Single(b => b.Id == id);
            if (budgets_expenses == null)
            {
                return HttpNotFound();
            }
            return View(budgets_expenses);
        }

        //
        // POST: /Expenses/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Budgets_Expenses budgets_expenses = db.Budgets_Expenses.Single(b => b.Id == id);
            db.Budgets_Expenses.DeleteObject(budgets_expenses);
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