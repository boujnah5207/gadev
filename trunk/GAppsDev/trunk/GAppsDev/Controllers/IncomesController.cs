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
    public class IncomesController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /Income/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var budgets_incomes = db.Budgets_Incomes.Include("Budget").Include("Budgets_Incomes_types").Include("Budgets_Incomes_Institutions");
            return View(budgets_incomes.ToList());
        }

        //
        // GET: /Income/Details/5

        [OpenIdAuthorize]
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
        // GET: /Income/Create

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            ViewBag.BudgetId = new SelectList(db.Budgets, "Id", "Id");
            ViewBag.BudgetIncomeTypeId = new SelectList(db.Budgets_Incomes_types, "Id", "Name");
            ViewBag.BudgetsIncomeInstitutions = new SelectList(db.Budgets_Incomes_Institutions, "Id", "Name");
            return View();
        }

        //
        // POST: /Income/Create

        [OpenIdAuthorize]
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
            ViewBag.BudgetIncomeTypeId = new SelectList(db.Budgets_Incomes_types, "Id", "Name", budgets_incomes.BudgetIncomeTypeId);
            ViewBag.BudgetsIncomeInstitutions = new SelectList(db.Budgets_Incomes_Institutions, "Id", "Name", budgets_incomes.BudgetsIncomeInstitutions);
            return View(budgets_incomes);
        }

        //
        // GET: /Income/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            Budgets_Incomes budgets_incomes = db.Budgets_Incomes.Single(b => b.Id == id);
            if (budgets_incomes == null)
            {
                return HttpNotFound();
            }
            ViewBag.BudgetId = new SelectList(db.Budgets, "Id", "Id", budgets_incomes.BudgetId);
            ViewBag.BudgetIncomeTypeId = new SelectList(db.Budgets_Incomes_types, "Id", "Name", budgets_incomes.BudgetIncomeTypeId);
            ViewBag.BudgetsIncomeInstitutions = new SelectList(db.Budgets_Incomes_Institutions, "Id", "Name", budgets_incomes.BudgetsIncomeInstitutions);
            return View(budgets_incomes);
        }

        //
        // POST: /Income/Edit/5

        [OpenIdAuthorize]
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
            ViewBag.BudgetIncomeTypeId = new SelectList(db.Budgets_Incomes_types, "Id", "Name", budgets_incomes.BudgetIncomeTypeId);
            ViewBag.BudgetsIncomeInstitutions = new SelectList(db.Budgets_Incomes_Institutions, "Id", "Name", budgets_incomes.BudgetsIncomeInstitutions);
            return View(budgets_incomes);
        }

        //
        // GET: /Income/Delete/5

        [OpenIdAuthorize]
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
        // POST: /Income/Delete/5

        [OpenIdAuthorize]
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