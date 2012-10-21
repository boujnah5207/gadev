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
    public class BudgetsExpensesController : Controller
    {
        private Entities db = new Entities();

        //
        // GET: /BudgetsExpenses/

        public ActionResult Index()
        {
            return View(db.Budgets_Expenses.ToList());
        }

        //
        // GET: /BudgetsExpenses/Details/5

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
        // GET: /BudgetsExpenses/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /BudgetsExpenses/Create

        [HttpPost]
        public ActionResult Create(Budgets_Expenses budgets_expenses)
        {
            if (ModelState.IsValid)
            {
                db.Budgets_Expenses.AddObject(budgets_expenses);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(budgets_expenses);
        }

        //
        // GET: /BudgetsExpenses/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Budgets_Expenses budgets_expenses = db.Budgets_Expenses.Single(b => b.Id == id);
            if (budgets_expenses == null)
            {
                return HttpNotFound();
            }
            return View(budgets_expenses);
        }

        //
        // POST: /BudgetsExpenses/Edit/5

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
            return View(budgets_expenses);
        }

        //
        // GET: /BudgetsExpenses/Delete/5

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
        // POST: /BudgetsExpenses/Delete/5

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