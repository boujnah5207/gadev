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
    public class BudgetAllocationsController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /BudgetAllocations/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var budgets_expensestoincomes = db.Budgets_ExpensesToIncomes.Include("Budgets_Expenses").Include("Budgets_Incomes");
            return View(budgets_expensestoincomes.ToList());
        }

        //
        // GET: /BudgetAllocations/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            Budgets_ExpensesToIncomes budgets_expensestoincomes = db.Budgets_ExpensesToIncomes.Single(b => b.Id == id);
            if (budgets_expensestoincomes == null)
            {
                return HttpNotFound();
            }
            return View(budgets_expensestoincomes);
        }

        //
        // GET: /BudgetAllocations/Create

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            ViewBag.ExpenseId = new SelectList(db.Budgets_Expenses, "Id", "CustomName");
            ViewBag.IncomeId = new SelectList(db.Budgets_Incomes, "Id", "CustomName");
            return View();
        }

        //
        // POST: /BudgetAllocations/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Budgets_ExpensesToIncomes budgets_expensestoincomes)
        {
            if (ModelState.IsValid)
            {
                db.Budgets_ExpensesToIncomes.AddObject(budgets_expensestoincomes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ExpenseId = new SelectList(db.Budgets_Expenses, "Id", "CustomName", budgets_expensestoincomes.ExpenseId);
            ViewBag.IncomeId = new SelectList(db.Budgets_Incomes, "Id", "CustomName", budgets_expensestoincomes.IncomeId);
            return View(budgets_expensestoincomes);
        }

        //
        // GET: /BudgetAllocations/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            Budgets_ExpensesToIncomes budgets_expensestoincomes = db.Budgets_ExpensesToIncomes.Single(b => b.Id == id);
            if (budgets_expensestoincomes == null)
            {
                return HttpNotFound();
            }
            ViewBag.ExpenseId = new SelectList(db.Budgets_Expenses, "Id", "CustomName", budgets_expensestoincomes.ExpenseId);
            ViewBag.IncomeId = new SelectList(db.Budgets_Incomes, "Id", "CustomName", budgets_expensestoincomes.IncomeId);
            return View(budgets_expensestoincomes);
        }

        //
        // POST: /BudgetAllocations/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Budgets_ExpensesToIncomes budgets_expensestoincomes)
        {
            if (ModelState.IsValid)
            {
                db.Budgets_ExpensesToIncomes.Attach(budgets_expensestoincomes);
                db.ObjectStateManager.ChangeObjectState(budgets_expensestoincomes, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ExpenseId = new SelectList(db.Budgets_Expenses, "Id", "CustomName", budgets_expensestoincomes.ExpenseId);
            ViewBag.IncomeId = new SelectList(db.Budgets_Incomes, "Id", "CustomName", budgets_expensestoincomes.IncomeId);
            return View(budgets_expensestoincomes);
        }

        //
        // GET: /BudgetAllocations/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            Budgets_ExpensesToIncomes budgets_expensestoincomes = db.Budgets_ExpensesToIncomes.Single(b => b.Id == id);
            if (budgets_expensestoincomes == null)
            {
                return HttpNotFound();
            }
            return View(budgets_expensestoincomes);
        }

        //
        // POST: /BudgetAllocations/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Budgets_ExpensesToIncomes budgets_expensestoincomes = db.Budgets_ExpensesToIncomes.Single(b => b.Id == id);
            db.Budgets_ExpensesToIncomes.DeleteObject(budgets_expensestoincomes);
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