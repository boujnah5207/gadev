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
    public class BudgetAllocationController : Controller
    {
        private Entities db = new Entities();

        //
        // GET: /BudgetAllocation/

        public ActionResult Index()
        {
            var budget_expensestoincome = db.Budget_ExpensesToIncome.Include("Budget_Expenses").Include("Budget_Income");
            return View(budget_expensestoincome.ToList());
        }

        //
        // GET: /BudgetAllocation/Details/5

        public ActionResult Details(int id = 0)
        {
            Budget_ExpensesToIncome budget_expensestoincome = db.Budget_ExpensesToIncome.Single(b => b.Id == id);
            if (budget_expensestoincome == null)
            {
                return HttpNotFound();
            }
            return View(budget_expensestoincome);
        }

        //
        // GET: /BudgetAllocation/Create

        public ActionResult Create()
        {
            ViewBag.ExpenseId = new SelectList(db.Budget_Expenses, "Id", "SectionName");
            ViewBag.IncomeId = new SelectList(db.Budget_Income, "Id", "Name");
            return View();
        }

        //
        // POST: /BudgetAllocation/Create

        [HttpPost]
        public ActionResult Create(Budget_ExpensesToIncome budget_expensestoincome)
        {
            if (ModelState.IsValid)
            {
                db.Budget_ExpensesToIncome.AddObject(budget_expensestoincome);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ExpenseId = new SelectList(db.Budget_Expenses, "Id", "SectionName", budget_expensestoincome.ExpenseId);
            ViewBag.IncomeId = new SelectList(db.Budget_Income, "Id", "Name", budget_expensestoincome.IncomeId);
            return View(budget_expensestoincome);
        }

        //
        // GET: /BudgetAllocation/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Budget_ExpensesToIncome budget_expensestoincome = db.Budget_ExpensesToIncome.Single(b => b.Id == id);
            if (budget_expensestoincome == null)
            {
                return HttpNotFound();
            }
            ViewBag.ExpenseId = new SelectList(db.Budget_Expenses, "Id", "SectionName", budget_expensestoincome.ExpenseId);
            ViewBag.IncomeId = new SelectList(db.Budget_Income, "Id", "Name", budget_expensestoincome.IncomeId);
            return View(budget_expensestoincome);
        }

        //
        // POST: /BudgetAllocation/Edit/5

        [HttpPost]
        public ActionResult Edit(Budget_ExpensesToIncome budget_expensestoincome)
        {
            if (ModelState.IsValid)
            {
                db.Budget_ExpensesToIncome.Attach(budget_expensestoincome);
                db.ObjectStateManager.ChangeObjectState(budget_expensestoincome, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ExpenseId = new SelectList(db.Budget_Expenses, "Id", "SectionName", budget_expensestoincome.ExpenseId);
            ViewBag.IncomeId = new SelectList(db.Budget_Income, "Id", "Name", budget_expensestoincome.IncomeId);
            return View(budget_expensestoincome);
        }

        //
        // GET: /BudgetAllocation/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Budget_ExpensesToIncome budget_expensestoincome = db.Budget_ExpensesToIncome.Single(b => b.Id == id);
            if (budget_expensestoincome == null)
            {
                return HttpNotFound();
            }
            return View(budget_expensestoincome);
        }

        //
        // POST: /BudgetAllocation/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Budget_ExpensesToIncome budget_expensestoincome = db.Budget_ExpensesToIncome.Single(b => b.Id == id);
            db.Budget_ExpensesToIncome.DeleteObject(budget_expensestoincome);
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