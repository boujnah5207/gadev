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
    public class BudgetsExpensesToIncomesController : Controller
    {
        private Entities db = new Entities();

        //
        // GET: /BudgetsExpensesToIncomes/

        public ActionResult Index()
        {
            var budgets_expensestoincomes = db.Budgets_ExpensesToIncomes.Include("Budgets_Expenses").Include("Budgets_Incomes");
            return View(budgets_expensestoincomes.ToList());
        }

        //
        // GET: /BudgetsExpensesToIncomes/Details/5

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
        // GET: /BudgetsExpensesToIncomes/Create

        public ActionResult Create()
        {
            ViewBag.ExpenseId = new SelectList(db.Budgets_Expenses, "Id", "SectionName");
            ViewBag.IncomeId = new SelectList(db.Budgets_Incomes, "Id", "Name");
            return View();
        }

        //
        // POST: /BudgetsExpensesToIncomes/Create

        [HttpPost]
        public ActionResult Create(Budgets_ExpensesToIncomes budgets_expensestoincomes)
        {
            if (ModelState.IsValid)
            {
                db.Budgets_ExpensesToIncomes.AddObject(budgets_expensestoincomes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ExpenseId = new SelectList(db.Budgets_Expenses, "Id", "SectionName", budgets_expensestoincomes.ExpenseId);
            ViewBag.IncomeId = new SelectList(db.Budgets_Incomes, "Id", "Name", budgets_expensestoincomes.IncomeId);
            return View(budgets_expensestoincomes);
        }

        //
        // GET: /BudgetsExpensesToIncomes/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Budgets_ExpensesToIncomes budgets_expensestoincomes = db.Budgets_ExpensesToIncomes.Single(b => b.Id == id);
            if (budgets_expensestoincomes == null)
            {
                return HttpNotFound();
            }
            ViewBag.ExpenseId = new SelectList(db.Budgets_Expenses, "Id", "SectionName", budgets_expensestoincomes.ExpenseId);
            ViewBag.IncomeId = new SelectList(db.Budgets_Incomes, "Id", "Name", budgets_expensestoincomes.IncomeId);
            return View(budgets_expensestoincomes);
        }

        //
        // POST: /BudgetsExpensesToIncomes/Edit/5

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
            ViewBag.ExpenseId = new SelectList(db.Budgets_Expenses, "Id", "SectionName", budgets_expensestoincomes.ExpenseId);
            ViewBag.IncomeId = new SelectList(db.Budgets_Incomes, "Id", "Name", budgets_expensestoincomes.IncomeId);
            return View(budgets_expensestoincomes);
        }

        //
        // GET: /BudgetsExpensesToIncomes/Delete/5

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
        // POST: /BudgetsExpensesToIncomes/Delete/5

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