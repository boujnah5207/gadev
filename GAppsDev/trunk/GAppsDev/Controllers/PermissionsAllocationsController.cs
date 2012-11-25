using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DB;
using DA;
using Mvc4.OpenId.Sample.Security;

namespace GAppsDev.Controllers
{
    public class PermissionsAllocationsController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /PermissionsAllocations/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var budgets_permissionstoallocation = db.Budgets_PermissionsToAllocation.Include("Budgets_Allocations").Include("Budgets_Permissions");
            return View(budgets_permissionstoallocation.ToList());
        }

        //
        // GET: /PermissionsAllocations/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            Budgets_PermissionsToAllocation budgets_permissionstoallocation = db.Budgets_PermissionsToAllocation.Single(b => b.Id == id);
            if (budgets_permissionstoallocation == null)
            {
                return HttpNotFound();
            }
            return View(budgets_permissionstoallocation);
        }

        [OpenIdAuthorize]
        public ActionResult PermissionAllocationList(int permissionId, int budgetId)
        {
            using (BudgetsPermissionsToAllocationRepository perAlloRep = new BudgetsPermissionsToAllocationRepository())
            {
                List<Budgets_PermissionsToAllocation> perAlloList = perAlloRep.GetList("Budgets_Allocations").Where(x => x.BudgetId == budgetId).Where(x => x.BudgetsPermissionsId == permissionId).ToList();
                ViewBag.budgetYear = perAlloList[0].Budget.Year;
                ViewBag.PermissionName = perAlloList[0].Budgets_Permissions.Name;
                ViewBag.PermissionId = permissionId;
                ViewBag.BudgetId = budgetId;

                return View(perAlloList);  
                
            }
        }
        //
        // GET: /PermissionsAllocations/Create

    /*    public ActionResult Create()
        {
            ViewBag.BudgetsExpensesToIncomesId = new SelectList(db.Budgets_Allocations, "Id", "Id");
            ViewBag.BudgetsPermissionsId = new SelectList(db.Budgets_Permissions, "Id", "Name");
            return View();
        }*/

        [OpenIdAuthorize]
        public ActionResult Create(int permissionId, int budgetId)
        {
            Budgets_PermissionsToAllocation perAlloc = new Budgets_PermissionsToAllocation();
            using (BudgetsRepository budgetsRepository = new BudgetsRepository())
            using (BudgetsPermissionsRepository permissionsRepository = new BudgetsPermissionsRepository())
            using (AllocationRepository allocationRepository = new AllocationRepository())
            {
                ViewBag.AllocationList = new SelectList(allocationRepository.GetList().Where(x => x.BudgetId == budgetId).ToList(), "Id", "Name");
                //ViewBag.BudgetsExpensesToIncomesId = new SelectList(db.Budgets_Allocations, "Id", "Id");
                perAlloc.BudgetId = budgetId;
                perAlloc.BudgetsPermissionsId = permissionId;
                Budget budget = budgetsRepository.GetEntity(budgetId);
                ViewBag.budgetYear = budget.Year;
                Budgets_Permissions permission = permissionsRepository.GetEntity(permissionId);
                ViewBag.PermissionName = permission.Name;
                
            }
            return View(perAlloc);
        }
        //
        // POST: /PermissionsAllocations/Create
        
        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Budgets_PermissionsToAllocation budgets_permissionstoallocation)
        {
            using (BudgetsPermissionsToAllocationRepository perToAllRep = new BudgetsPermissionsToAllocationRepository())
            {
                perToAllRep.Create(budgets_permissionstoallocation);
                return RedirectToAction("PermissionAllocationList", "PermissionsAllocations", new { permissionId = budgets_permissionstoallocation.BudgetsPermissionsId, budgetId = budgets_permissionstoallocation.BudgetId });
            }

        }

        //
        // GET: /PermissionsAllocations/Edit/5
        [OpenIdAuthorize]
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
        [OpenIdAuthorize]
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
        [OpenIdAuthorize]
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
        [OpenIdAuthorize]
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