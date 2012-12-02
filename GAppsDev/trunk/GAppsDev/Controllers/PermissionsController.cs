﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using DB;
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
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);

            List<Budgets_Baskets> permissions = new List<Budgets_Baskets>();
            using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
            {
                permissions = permissionsRep.GetList("Budgets_BasketsToAllocation").Where(x => x.CompanyId == CurrentUser.CompanyId).ToList();
            }
            return View(permissions);
        }

        [OpenIdAuthorize]
        public ActionResult ExistPermissionsList(int id = 0)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);
            
            List<Budgets_Baskets> model;
            Budgets_BasketsToAllocation per = new Budgets_BasketsToAllocation();
            using (BudgetsRepository budgetsRep = new BudgetsRepository())
            using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
            {
                model = permissionsRep.GetList("Budgets_BasketsToAllocation").Where(x => x.CompanyId == CurrentUser.CompanyId).ToList();
                ViewBag.budgetId = id;
                Budget budget = budgetsRep.GetList().SingleOrDefault(x => x.Id == id);
                ViewBag.budgetYear = budget.Year;
                ViewBag.budgetId = budget.Id;

            }
            return View(model);
        }

        //
        // GET: /Permissions/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Baskets permission;
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
                        return Error(Loc.Dic.error_no_permission);
                    }
                }
                else
                {
                    return Error(Loc.Dic.error_permissions_get_error);
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // GET: /Permissions/Create

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            if (Authorized(RoleType.SystemManager))
            {
                return View();
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // POST: /Permissions/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Budgets_Baskets budgets_permissions, int budgetId = 0)
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
                        return RedirectToAction("Index", new { id = budgetId });
                    else
                        return Error(Loc.Dic.error_permissions_create_error);
                }
                else
                {
                    return Error(Loc.Dic.error_invalid_form);
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // GET: /Permissions/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Baskets permission;
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
                        return Error(Loc.Dic.error_no_permission);
                    }
                }
                else
                {
                    return Error(Loc.Dic.error_permissions_get_error);
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // POST: /Permissions/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Budgets_Baskets budgets_permissions)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Baskets permissionFromDB;
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
                            return Error(Loc.Dic.error_no_permission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_permissions_get_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        [OpenIdAuthorize]
        public ActionResult EditAllocations(int id = 0, int budgetId = 0)
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
                                    List<SelectListItemDB> allocationsList = budget.Budgets_Allocations
                                        .Select(a => new { Id = a.Id, Name = a.Name })
                                        .AsEnumerable()
                                        .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name.ToString() })
                                        .ToList();

                                    List<PermissionAllocation> permissionsToAllocations = permissionsAllocationsRep.GetList("Budgets_Allocations", "Budgets_Allocations.Budgets_Incomes", "Budgets_Allocations.Budgets_Expenses")
                                        .Where(x => x.BudgetId == budget.Id && x.BasketId == model.Permission.Id)
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
                                return Error(Loc.Dic.error_database_error);
                            }

                            return View(model);
                        }
                        else
                        {
                            return Error(Loc.Dic.error_no_permission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_permissions_get_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
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
                Budgets_Baskets permissionFromDB;
                List<Budgets_Allocations> existingPermissionAllocations;
                List<Budgets_BasketsToAllocation> existingPermissionToAllocations;

                using (BudgetsRepository budgetsRep = new BudgetsRepository())
                using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
                using (BudgetsPermissionsToAllocationRepository permissionsAllocationsRep = new BudgetsPermissionsToAllocationRepository())
                using (AllocationRepository allocationsRep = new AllocationRepository())
                {
                    permissionFromDB = permissionsRep.GetEntity(model.Permission.Id);
                    //TODO: Error gets ALL pemissions from DB
                    existingPermissionAllocations = permissionsAllocationsRep.GetList().Where(x => x.BasketId == permissionFromDB.Id).Select(y => y.Budgets_Allocations).ToList();
                    existingPermissionToAllocations = permissionsAllocationsRep.GetList().Where(x => x.BasketId == permissionFromDB.Id).ToList();

                    if (permissionFromDB != null)
                    {
                        if (permissionFromDB.CompanyId == CurrentUser.CompanyId)
                        {
                            foreach (var budgetAllocation in model.BudgetAllocationsList)
                            {
                                Budget budgetFromDB = budgetsRep.GetEntity(budgetAllocation.Budget.Id);

                                if (budgetFromDB != null && budgetFromDB.CompanyId == CurrentUser.CompanyId)
                                {
                                    foreach (var allocation in budgetAllocation.PermissionAllocations)
                                    {
                                        if (allocation.IsActive)
                                        {
                                            if (!existingPermissionAllocations.Any(x => x.Id == allocation.Allocation.BudgetsExpensesToIncomesId))
                                            {
                                                allocation.Allocation.BudgetId = budgetFromDB.Id;
                                                allocation.Allocation.BasketId = permissionFromDB.Id;
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
                            return Error(Loc.Dic.error_no_permission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_permissions_get_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // GET: /Permissions/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0, int budgetId = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Baskets permission;

                using (BudgetsPermissionsRepository permissiosRep = new BudgetsPermissionsRepository())
                {
                    permission = permissiosRep.GetEntity(id);

                    if (permission != null)
                    {
                        if (permission.CompanyId == CurrentUser.CompanyId)
                        {
                            ViewBag.BudgetId = budgetId;
                            return View(permission);
                        }
                        else
                        {
                            return Error(Loc.Dic.error_no_permission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_projects_get_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // POST: /Permissions/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id, int budgetId)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Baskets permission;

                using (OrdersRepository orderssRep = new OrdersRepository(CurrentUser.CompanyId))
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
                            List<int> permissionAllocations = permission.Budgets_BasketsToAllocation.Select(x => x.Id).ToList();
                            List<int> usersPermissions = permission.Budgets_UsersToBaskets.Select(x => x.Id).ToList();

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
                                return RedirectToAction("Index", new { id = budgetId });
                            else
                                return Error(Loc.Dic.error_permissions_delete_error);
                        }
                        else
                        {
                            return Error(Loc.Dic.error_no_permission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_projects_get_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}