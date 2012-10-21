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
    public class BudgetsIncomesController : Controller
    {
        private Entities db = new Entities();

        //
        // GET: /BudgetsIncomes/

        public ActionResult Index()
        {
            var budgets_incomes = db.Budgets_Incomes.Include("Budget");
            return View(budgets_incomes.ToList());
        }

        //
        // GET: /BudgetsIncomes/Details/5

        public ActionResult Details(int id = 0)
        {
            Budgets_Incomes budgets_incomes = db.Budgets_Incomes.Single(b => b.Id == id);
            if (budgets_incomes == null)
            {
                return HttpNotFound();
            }
            return View(budgets_incomes);
        }

        //
        // GET: /BudgetsIncomes/Create

        public ActionResult Create()
        {
            ViewBag.BudgetId = new SelectList(db.Budgets, "Id", "Id");
            return View();
        }

        //
        // POST: /BudgetsIncomes/Create

        [HttpPost]
        public ActionResult Create(Budgets_Incomes budgets_incomes)
        {
            if (ModelState.IsValid)
            {
                db.Budgets_Incomes.AddObject(budgets_incomes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BudgetId = new SelectList(db.Budgets, "Id", "Id", budgets_incomes.BudgetId);
            return View(budgets_incomes);
        }

        //
        // GET: /BudgetsIncomes/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Budgets_Incomes budgets_incomes = db.Budgets_Incomes.Single(b => b.Id == id);
            if (budgets_incomes == null)
            {
                return HttpNotFound();
            }
            ViewBag.BudgetId = new SelectList(db.Budgets, "Id", "Id", budgets_incomes.BudgetId);
            return View(budgets_incomes);
        }

        //
        // POST: /BudgetsIncomes/Edit/5

        [HttpPost]
        public ActionResult Edit(Budgets_Incomes budgets_incomes)
        {
            if (ModelState.IsValid)
            {
                db.Budgets_Incomes.Attach(budgets_incomes);
                db.ObjectStateManager.ChangeObjectState(budgets_incomes, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BudgetId = new SelectList(db.Budgets, "Id", "Id", budgets_incomes.BudgetId);
            return View(budgets_incomes);
        }

        //
        // GET: /BudgetsIncomes/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Budgets_Incomes budgets_incomes = db.Budgets_Incomes.Single(b => b.Id == id);
            if (budgets_incomes == null)
            {
                return HttpNotFound();
            }
            return View(budgets_incomes);
        }

        //
        // POST: /BudgetsIncomes/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Budgets_Incomes budgets_incomes = db.Budgets_Incomes.Single(b => b.Id == id);
            db.Budgets_Incomes.DeleteObject(budgets_incomes);
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