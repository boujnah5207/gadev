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
    public class BudgetExpensesController : Controller
    {
        private Entities db = new Entities();

        //
        // GET: /BudgetExpenses/

        public ActionResult Index()
        {
            var budget_expenses = db.Budget_Expenses.Include("Company").Include("Budget_Departments");
            return View(budget_expenses.ToList());
        }

        //
        // GET: /BudgetExpenses/Details/5

        public ActionResult Details(int id = 0)
        {
            Budget_Expenses budget_expenses = db.Budget_Expenses.Single(b => b.Id == id);
            if (budget_expenses == null)
            {
                return HttpNotFound();
            }
            return View(budget_expenses);
        }

        //
        // GET: /BudgetExpenses/Create

        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name");
            ViewBag.DepartmentId = new SelectList(db.Budget_Departments, "Id", "Id");
            return View();
        }

        //
        // POST: /BudgetExpenses/Create

        [HttpPost]
        public ActionResult Create(Budget_Expenses budget_expenses)
        {
            if (ModelState.IsValid)
            {
                db.Budget_Expenses.AddObject(budget_expenses);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", budget_expenses.CompanyId);
            ViewBag.DepartmentId = new SelectList(db.Budget_Departments, "Id", "Id", budget_expenses.DepartmentId);
            return View(budget_expenses);
        }

        //
        // GET: /BudgetExpenses/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Budget_Expenses budget_expenses = db.Budget_Expenses.Single(b => b.Id == id);
            if (budget_expenses == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", budget_expenses.CompanyId);
            ViewBag.DepartmentId = new SelectList(db.Budget_Departments, "Id", "Id", budget_expenses.DepartmentId);
            return View(budget_expenses);
        }

        //
        // POST: /BudgetExpenses/Edit/5

        [HttpPost]
        public ActionResult Edit(Budget_Expenses budget_expenses)
        {
            if (ModelState.IsValid)
            {
                db.Budget_Expenses.Attach(budget_expenses);
                db.ObjectStateManager.ChangeObjectState(budget_expenses, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", budget_expenses.CompanyId);
            ViewBag.DepartmentId = new SelectList(db.Budget_Departments, "Id", "Id", budget_expenses.DepartmentId);
            return View(budget_expenses);
        }

        //
        // GET: /BudgetExpenses/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Budget_Expenses budget_expenses = db.Budget_Expenses.Single(b => b.Id == id);
            if (budget_expenses == null)
            {
                return HttpNotFound();
            }
            return View(budget_expenses);
        }

        //
        // POST: /BudgetExpenses/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Budget_Expenses budget_expenses = db.Budget_Expenses.Single(b => b.Id == id);
            db.Budget_Expenses.DeleteObject(budget_expenses);
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