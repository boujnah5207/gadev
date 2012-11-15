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
    public class PermissionsAllocationsController : Controller
    {
        private Entities db = new Entities();

        //
        // GET: /PermissionsAllocations/

        public ActionResult Index()
        {
            var budgets_permissionstoallocation = db.Budgets_PermissionsToAllocation.Include("Budgets_Allocations").Include("Budgets_Permissions");
            return View(budgets_permissionstoallocation.ToList());
        }

        //
        // GET: /PermissionsAllocations/Details/5

        public ActionResult Details(int id = 0)
        {
            Budgets_PermissionsToAllocation budgets_permissionstoallocation = db.Budgets_PermissionsToAllocation.Single(b => b.Id == id);
            if (budgets_permissionstoallocation == null)
            {
                return HttpNotFound();
            }
            return View(budgets_permissionstoallocation);
        }

        //
        // GET: /PermissionsAllocations/Create

        public ActionResult Create()
        {
            ViewBag.BudgetsExpensesToIncomesId = new SelectList(db.Budgets_Allocations, "Id", "Id");
            ViewBag.BudgetsPermissionsId = new SelectList(db.Budgets_Permissions, "Id", "Name");
            return View();
        }

        //
        // POST: /PermissionsAllocations/Create

        [HttpPost]
        public ActionResult Create(Budgets_PermissionsToAllocation budgets_permissionstoallocation)
        {
            if (ModelState.IsValid)
            {
                db.Budgets_PermissionsToAllocation.AddObject(budgets_permissionstoallocation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BudgetsExpensesToIncomesId = new SelectList(db.Budgets_Allocations, "Id", "Id", budgets_permissionstoallocation.BudgetsExpensesToIncomesId);
            ViewBag.BudgetsPermissionsId = new SelectList(db.Budgets_Permissions, "Id", "Name", budgets_permissionstoallocation.BudgetsPermissionsId);
            return View(budgets_permissionstoallocation);
        }

        //
        // GET: /PermissionsAllocations/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Budgets_PermissionsToAllocation budgets_permissionstoallocation = db.Budgets_PermissionsToAllocation.Single(b => b.Id == id);
            if (budgets_permissionstoallocation == null)
            {
                return HttpNotFound();
            }
            ViewBag.BudgetsExpensesToIncomesId = new SelectList(db.Budgets_Allocations, "Id", "Id", budgets_permissionstoallocation.BudgetsExpensesToIncomesId);
            ViewBag.BudgetsPermissionsId = new SelectList(db.Budgets_Permissions, "Id", "Name", budgets_permissionstoallocation.BudgetsPermissionsId);
            return View(budgets_permissionstoallocation);
        }

        //
        // POST: /PermissionsAllocations/Edit/5

        [HttpPost]
        public ActionResult Edit(Budgets_PermissionsToAllocation budgets_permissionstoallocation)
        {
            if (ModelState.IsValid)
            {
                db.Budgets_PermissionsToAllocation.Attach(budgets_permissionstoallocation);
                db.ObjectStateManager.ChangeObjectState(budgets_permissionstoallocation, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BudgetsExpensesToIncomesId = new SelectList(db.Budgets_Allocations, "Id", "Id", budgets_permissionstoallocation.BudgetsExpensesToIncomesId);
            ViewBag.BudgetsPermissionsId = new SelectList(db.Budgets_Permissions, "Id", "Name", budgets_permissionstoallocation.BudgetsPermissionsId);
            return View(budgets_permissionstoallocation);
        }

        //
        // GET: /PermissionsAllocations/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Budgets_PermissionsToAllocation budgets_permissionstoallocation = db.Budgets_PermissionsToAllocation.Single(b => b.Id == id);
            if (budgets_permissionstoallocation == null)
            {
                return HttpNotFound();
            }
            return View(budgets_permissionstoallocation);
        }

        //
        // POST: /PermissionsAllocations/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Budgets_PermissionsToAllocation budgets_permissionstoallocation = db.Budgets_PermissionsToAllocation.Single(b => b.Id == id);
            db.Budgets_PermissionsToAllocation.DeleteObject(budgets_permissionstoallocation);
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