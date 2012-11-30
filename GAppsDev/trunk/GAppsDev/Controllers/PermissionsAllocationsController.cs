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
            var budgets_permissionstoallocation = db.Budgets_BasketsToAllocation.Include("Budgets_Allocations").Include("Budgets_Baskets");
            return View(budgets_permissionstoallocation.ToList());
        }

        //
        // GET: /PermissionsAllocations/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            Budgets_BasketsToAllocation budgets_permissionstoallocation = db.Budgets_BasketsToAllocation.Single(b => b.Id == id);
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
                List<Budgets_BasketsToAllocation> perAlloList = perAlloRep.GetList("Budgets_Allocations").Where(x => x.BudgetId == budgetId).Where(x => x.BasketId == permissionId).ToList();
                ViewBag.budgetYear = perAlloList[0].Budget.Year;
                ViewBag.PermissionName = perAlloList[0].Budgets_Baskets.Name;
                ViewBag.BasketId = permissionId;
                ViewBag.BudgetId = budgetId;

                return View(perAlloList);  
                
            }
        }
        //
        // GET: /PermissionsAllocations/Create

    /*    public ActionResult Create()
        {
            ViewBag.BudgetsExpensesToIncomesId = new SelectList(db.Budgets_Allocations, "Id", "Id");
            ViewBag.BasketId = new SelectList(db.Budgets_Baskets, "Id", "Name");
            return View();
        }*/

        [OpenIdAuthorize]
        public ActionResult Create(int permissionId, int budgetId)
        {
            Budgets_BasketsToAllocation perAlloc = new Budgets_BasketsToAllocation();
            using (BudgetsRepository budgetsRepository = new BudgetsRepository())
            using (BudgetsPermissionsRepository permissionsRepository = new BudgetsPermissionsRepository())
            using (AllocationRepository allocationRepository = new AllocationRepository())
            {
                ViewBag.AllocationList = new SelectList(allocationRepository.GetList().Where(x => x.BudgetId == budgetId).OrderBy( x => x.ExternalId).ToList(), "Id", "DisplayName");
                //ViewBag.BudgetsExpensesToIncomesId = new SelectList(db.Budgets_Allocations, "Id", "Id");
                perAlloc.BudgetId = budgetId;
                perAlloc.BasketId = permissionId;
                Budget budget = budgetsRepository.GetEntity(budgetId);
                ViewBag.budgetYear = budget.Year;
                Budgets_Baskets permission = permissionsRepository.GetEntity(permissionId);
                ViewBag.PermissionName = permission.Name;
                
            }
            return View(perAlloc);
        }
        //
        // POST: /PermissionsAllocations/Create
        
        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Budgets_BasketsToAllocation budgets_permissionstoallocation)
        {
            using (BudgetsPermissionsToAllocationRepository perToAllRep = new BudgetsPermissionsToAllocationRepository())
            {
                perToAllRep.Create(budgets_permissionstoallocation);
                return RedirectToAction("PermissionAllocationList", "PermissionsAllocations", new { permissionId = budgets_permissionstoallocation.BasketId, budgetId = budgets_permissionstoallocation.BudgetId });
            }

        }

        //
        // GET: /PermissionsAllocations/Edit/5
        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            Budgets_BasketsToAllocation budgets_permissionstoallocation = db.Budgets_BasketsToAllocation.Single(b => b.Id == id);
            if (budgets_permissionstoallocation == null)
            {
                return HttpNotFound();
            }
            ViewBag.BudgetsExpensesToIncomesId = new SelectList(db.Budgets_Allocations, "Id", "Id", budgets_permissionstoallocation.BudgetsExpensesToIncomesId);
            ViewBag.BasketId = new SelectList(db.Budgets_Baskets, "Id", "Name", budgets_permissionstoallocation.BasketId);
            return View(budgets_permissionstoallocation);
        }

        //
        // POST: /PermissionsAllocations/Edit/5
        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Budgets_BasketsToAllocation budgets_permissionstoallocation)
        {
            if (ModelState.IsValid)
            {
                db.Budgets_BasketsToAllocation.Attach(budgets_permissionstoallocation);
                db.ObjectStateManager.ChangeObjectState(budgets_permissionstoallocation, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BudgetsExpensesToIncomesId = new SelectList(db.Budgets_Allocations, "Id", "Id", budgets_permissionstoallocation.BudgetsExpensesToIncomesId);
            ViewBag.BasketId = new SelectList(db.Budgets_Baskets, "Id", "Name", budgets_permissionstoallocation.BasketId);
            return View(budgets_permissionstoallocation);
        }

        //
        // GET: /PermissionsAllocations/Delete/5
        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            Budgets_BasketsToAllocation budgets_permissionstoallocation = db.Budgets_BasketsToAllocation.Single(b => b.Id == id);
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
            Budgets_BasketsToAllocation budgets_permissionstoallocation = db.Budgets_BasketsToAllocation.Single(b => b.Id == id);
            db.Budgets_BasketsToAllocation.DeleteObject(budgets_permissionstoallocation);
            db.SaveChanges();
            return RedirectToAction("PermissionAllocationList", new { permissionId = budgets_permissionstoallocation.BasketId, budgetId = budgets_permissionstoallocation.BudgetId });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}