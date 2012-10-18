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
    public class BudgetIncomeController : Controller
    {
        private Entities db = new Entities();

        //
        // GET: /BudgetIncome/

        public ActionResult Index()
        {
            var budget_income = db.Budget_Income.Include("Company");
            return View(budget_income.ToList());
        }

        //
        // GET: /BudgetIncome/Details/5

        public ActionResult Details(int id = 0)
        {
            Budget_Income budget_income = db.Budget_Income.Single(b => b.Id == id);
            if (budget_income == null)
            {
                return HttpNotFound();
            }
            return View(budget_income);
        }

        //
        // GET: /BudgetIncome/Create

        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name");
            return View();
        }

        //
        // POST: /BudgetIncome/Create

        [HttpPost]
        public ActionResult Create(Budget_Income budget_income)
        {
            if (ModelState.IsValid)
            {
                db.Budget_Income.AddObject(budget_income);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", budget_income.CompanyId);
            return View(budget_income);
        }

        //
        // GET: /BudgetIncome/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Budget_Income budget_income = db.Budget_Income.Single(b => b.Id == id);
            if (budget_income == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", budget_income.CompanyId);
            return View(budget_income);
        }

        //
        // POST: /BudgetIncome/Edit/5

        [HttpPost]
        public ActionResult Edit(Budget_Income budget_income)
        {
            if (ModelState.IsValid)
            {
                db.Budget_Income.Attach(budget_income);
                db.ObjectStateManager.ChangeObjectState(budget_income, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", budget_income.CompanyId);
            return View(budget_income);
        }

        //
        // GET: /BudgetIncome/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Budget_Income budget_income = db.Budget_Income.Single(b => b.Id == id);
            if (budget_income == null)
            {
                return HttpNotFound();
            }
            return View(budget_income);
        }

        //
        // POST: /BudgetIncome/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Budget_Income budget_income = db.Budget_Income.Single(b => b.Id == id);
            db.Budget_Income.DeleteObject(budget_income);
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