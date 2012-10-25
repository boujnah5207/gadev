using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using DB;
using GAppsDev.Models.ErrorModels;
using GAppsDev.Models.PermissionModels;
using Mvc4.OpenId.Sample.Security;

namespace GAppsDev.Controllers
{
    public class PermissionsController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /Permissions/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            if (Authorized(RoleType.SystemManager))
            {
                List<Budgets_Permissions> model;

                using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
                {
                    model = permissionsRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).ToList();
                }

                return View(model);
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // GET: /Permissions/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Permissions permission;
                using (BudgetsPermissionsRepository permissionRep = new BudgetsPermissionsRepository())
                {
                    permission = permissionRep.GetEntity(id);
                }

                if (permission != null)
                {
                    if (permission.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(permission);
                    }
                    else
                    {
                        return Error(Errors.NO_PERMISSION);
                    }
                }
                else
                {
                    return Error(Errors.PERMISSIONS_GET_ERROR);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // GET: /Permissions/Create

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            if (Authorized(RoleType.SystemManager))
            {
                List<SelectListItemDB> budgetsList;

                using (BudgetsRepository budgetRep = new BudgetsRepository())
                {
                    budgetsList = budgetRep.GetList()
                        .Where(budget => budget.CompanyId == CurrentUser.CompanyId && budget.Year >= (DateTime.Now.Year - 1))
                        .Select(a => new { Id = a.Id, Name = a.Year })
                        .AsEnumerable()
                        .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name.ToString() })
                        .ToList();
                }

                ViewBag.BudgetId = new SelectList(budgetsList, "Id", "Name");
                return View();
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Permissions/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Budgets_Permissions budgets_permissions)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (ModelState.IsValid)
                {
                    budgets_permissions.CompanyId = CurrentUser.CompanyId;

                    bool wasCreated;
                    using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
                    {
                        wasCreated = permissionsRep.Create(budgets_permissions);
                    }

                    if (wasCreated)
                        return RedirectToAction("Index");
                    else
                        return Error(Errors.PERMISSIONS_CREATE_ERROR);

                }
                else
                {
                    return Error(ModelState);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // GET: /Permissions/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Permissions permission;
                using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
                {
                    permission = permissionsRep.GetEntity(id);
                }

                if (permission != null)
                {
                    if (permission.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(permission);
                    }
                    else
                    {
                        return Error(Errors.NO_PERMISSION);
                    }
                }
                else
                {
                    return Error(Errors.PERMISSIONS_GET_ERROR);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Permissions/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Budgets_Permissions budgets_permissions)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Permissions permissionFromDB;
                using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
                {
                    permissionFromDB = permissionsRep.GetEntity(budgets_permissions.Id);

                    if (permissionFromDB != null)
                    {
                        if (permissionFromDB.CompanyId == CurrentUser.CompanyId)
                        {
                            permissionFromDB.Name = budgets_permissions.Name;

                            permissionsRep.Update(permissionFromDB);

                            return RedirectToAction("Index");
                        }
                        else
                        {
                            return Error(Errors.NO_PERMISSION);
                        }
                    }
                    else
                    {
                        return Error(Errors.PERMISSIONS_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [OpenIdAuthorize]
        public ActionResult EditAllocations(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                PermissionAllocationsModel model = new PermissionAllocationsModel();
                List<Budget> budgets;

                using (BudgetsRepository budgetsRep = new BudgetsRepository())
                using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
                using (BudgetsPermissionsToAllocationRepository permissionsAllocationsRep = new BudgetsPermissionsToAllocationRepository())
                {
                    model.Permission = permissionsRep.GetEntity(id);

                    if (model.Permission != null)
                    {
                        if (model.Permission.CompanyId == CurrentUser.CompanyId)
                        {
                            budgets = budgetsRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId && x.Year >= (DateTime.Now.Year - 1)).ToList();

                            if (budgets != null)
                            {
                                model.BudgetAllocationsList = new List<BudgetAllocations>();

                                foreach (var budget in budgets)
                                {
                                    List<SelectListItemDB> allocationsList = budget.Budgets_ExpensesToIncomes
                                        .Select(a => new { Id = a.Id, Name = a.Amount + ": " + a.Budgets_Incomes.CustomName + "-->" + a.Budgets_Expenses.CustomName })
                                        .AsEnumerable()
                                        .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name.ToString() })
                                        .ToList();

                                    List<PermissionAllocation> permissionsToAllocations = permissionsAllocationsRep.GetList("Budgets_ExpensesToIncomes", "Budgets_ExpensesToIncomes.Budgets_Incomes", "Budgets_ExpensesToIncomes.Budgets_Expenses")
                                        .Where(x => x.BudgetId == budget.Id)
                                        .AsEnumerable()
                                        .Select(alloc => new PermissionAllocation() { IsActive = true, Allocation = alloc })
                                        .ToList();

                                    BudgetAllocations newBudgetAllocations = new BudgetAllocations()
                                    {
                                        Budget = budget,
                                        AllocationsList = new SelectList(allocationsList, "Id", "Name"),
                                        PermissionAllocations = permissionsToAllocations
                                    };

                                    model.BudgetAllocationsList.Add(newBudgetAllocations);
                                }
                            }
                            else
                            {
                                return Error(Errors.DATABASE_ERROR);
                            }

                            return View(model);
                        }
                        else
                        {
                            return Error(Errors.NO_PERMISSION);
                        }
                    }
                    else
                    {
                        return Error(Errors.PERMISSIONS_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Permissions/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult EditAllocations(PermissionAllocationsModel model)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Permissions permissionFromDB;
                List<Budgets_ExpensesToIncomes> existingPermissionAllocations;

                using (BudgetsRepository budgetsRep = new BudgetsRepository())
                using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
                using (BudgetsPermissionsToAllocationRepository permissionsAllocationsRep = new BudgetsPermissionsToAllocationRepository())
                using (BudgetsExpensesToIncomesRepository allocationsRep = new BudgetsExpensesToIncomesRepository())
                {
                    permissionFromDB = permissionsRep.GetEntity(model.Permission.Id);
                    existingPermissionAllocations = permissionsAllocationsRep.GetList().Where(x => x.BudgetsPermissionsId == permissionFromDB.Id).Select( y => y.Budgets_ExpensesToIncomes).ToList();

                    if (permissionFromDB != null)
                    {
                        if (permissionFromDB.CompanyId == CurrentUser.CompanyId)
                        {
                            foreach (var budgetAllocation in model.BudgetAllocationsList)
	                        {
                                Budget budgetFromDB = budgetsRep.GetEntity(budgetAllocation.Budget.Id);

                                if(budgetFromDB != null && budgetFromDB.CompanyId == CurrentUser.CompanyId)
                                {
                                    foreach (var allocation in budgetAllocation.PermissionAllocations)
	                                {
		                                if(allocation.IsActive)
                                        {
                                            if(!existingPermissionAllocations.Any(x => x.Id == allocation.Allocation.BudgetsExpensesToIncomesId))
                                            {
                                                allocation.Allocation.BudgetId = budgetFromDB.Id;
                                                allocation.Allocation.BudgetsPermissionsId = permissionFromDB.Id;
                                                permissionsAllocationsRep.Create(allocation.Allocation);
                                            }
                                        }
                                        else
                                        {
                                            if (existingPermissionAllocations.Any(x => x.Id == allocation.Allocation.BudgetsExpensesToIncomesId))
                                            {
                                                permissionsAllocationsRep.Delete(allocation.Allocation.Id);
                                            }
                                        }
	                                }
                                }
                                else
                                {
                                    return Error("");
                                }
	                        }

                            return RedirectToAction("Index");
                        }
                        else
                        {
                            return Error(Errors.NO_PERMISSION);
                        }
                    }
                    else
                    {
                        return Error(Errors.PERMISSIONS_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // GET: /Permissions/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Permissions permission;

                using (BudgetsPermissionsRepository permissiosRep = new BudgetsPermissionsRepository())
                {
                    permission = permissiosRep.GetEntity(id);

                    if (permission != null)
                    {
                        if (permission.CompanyId == CurrentUser.CompanyId)
                        {
                            return View(permission);
                        }
                        else
                        {
                            return Error(Errors.NO_PERMISSION);
                        }
                    }
                    else
                    {
                        return Error(Errors.PROJECTS_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Permissions/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Permissions permission;

                using (OrdersRepository orderssRep = new OrdersRepository())
                using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
                using (BudgetsPermissionsToAllocationRepository permissionsAllocationsRep = new BudgetsPermissionsToAllocationRepository())
                using (BudgetsUsersToPermissionsRepository usersPermissionsRep = new BudgetsUsersToPermissionsRepository())
                {
                    permission = permissionsRep.GetEntity(id);

                    if (permission != null)
                    {
                        if (permission.CompanyId == CurrentUser.CompanyId)
                        {
                            bool noErrors = true;
                            List<int> permissionAllocations = permission.Budgets_PermissionsToAllocation.Select(x => x.Id).ToList();
                            List<int> usersPermissions = permission.Budgets_UsersToPermissions.Select(x => x.Id).ToList();

                            foreach (var itemId in permissionAllocations)
                            {
                                if (!permissionsAllocationsRep.Delete(itemId))
                                    noErrors = false;
                            }

                            foreach (var itemId in usersPermissions)
                            {
                                if (!usersPermissionsRep.Delete(itemId))
                                    noErrors = false;
                            }

                            if (!permissionsRep.Delete(permission.Id))
                                noErrors = false;

                            if (noErrors)
                                return RedirectToAction("Index");
                            else
                                return Error(Errors.PERMISSIONS_DELETE_ERROR);
                        }
                        else
                        {
                            return Error(Errors.NO_PERMISSION);
                        }
                    }
                    else
                    {
                        return Error(Errors.PROJECTS_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}