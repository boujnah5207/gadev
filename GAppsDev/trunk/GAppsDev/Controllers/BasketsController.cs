using System;
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
    public class BasketsController : BaseController
    {
        const int ITEMS_PER_PAGE = 10;
        const int FIRST_PAGE = 1;
        const string NO_SORT_BY = "None";
        const string DEFAULT_SORT = "name";
        const string DEFAULT_DESC_ORDER = "DESC";

        private Entities db = new Entities();

        //
        // GET: /Permissions/

        [OpenIdAuthorize]
        public ActionResult Index(int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);

            IEnumerable<Budgets_Baskets> permissions;
            using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
            {
                permissions = permissionsRep.GetList("Budgets_BasketsToAllocation").Where(x => x.CompanyId == CurrentUser.CompanyId);

                permissions = Pagination(permissions, page, sortby, order, true);

                return View(permissions.ToList());
            }
        }

        [OpenIdAuthorize]
        public ActionResult BudgetBaskets(int id = 0, int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);

            IEnumerable<Budgets_Baskets> baskets;
            Budgets_BasketsToAllocation per = new Budgets_BasketsToAllocation();

            using (BudgetsRepository budgetsRep = new BudgetsRepository(CurrentUser.CompanyId))
            using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
            {
                baskets = permissionsRep.GetList("Budgets_BasketsToAllocation").Where(x => x.CompanyId == CurrentUser.CompanyId);

                baskets = Pagination(baskets, page, sortby, order, true);

                ViewBag.budgetId = id;
                Budget budget = budgetsRep.GetList().SingleOrDefault(x => x.Id == id);
                ViewBag.budgetYear = budget.Year;
                ViewBag.budgetId = budget.Id;
                return View(baskets.ToList());
            }
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
            if (!Authorized(RoleType.SystemManager))
                return Error(Loc.Dic.error_no_permission);

            PermissionAllocationsModel model = new PermissionAllocationsModel();
            Budget budget;

            using (BudgetsRepository budgetsRep = new BudgetsRepository(CurrentUser.CompanyId))
            using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
            using (BasketsToAllocationsRepository permissionsAllocationsRep = new BasketsToAllocationsRepository())
            {
                model.Basket = permissionsRep.GetEntity(id);

                if (model.Basket == null)
                    return Error(Loc.Dic.error_permissions_get_error);

                if (model.Basket.CompanyId != CurrentUser.CompanyId)
                    return Error(Loc.Dic.error_no_permission);

                budget = budgetsRep.GetEntity(budgetId);

                if (budget == null)
                    return Error(Loc.Dic.error_database_error);

                List<PermissionAllocation> permissionsToAllocations = permissionsAllocationsRep.GetList("Budgets_Allocations", "Budgets_Allocations.Budgets_Incomes", "Budgets_Allocations.Budgets_Expenses")
                    .Where(x => x.BudgetId == budget.Id && x.BasketId == model.Basket.Id)
                    .AsEnumerable()
                    .Select(alloc => new PermissionAllocation() { IsActive = true, Allocation = alloc })
                    .ToList();

                model.BudgetAllocations = new BudgetAllocations()
                {
                    Budget = budget,
                    AllocationsList = budget.Budgets_Allocations.OrderBy(x=>x.SortingCode).ToList(),
                    PermissionAllocations = permissionsToAllocations
                };

                return View(model);
            }
        }

        //
        // POST: /Permissions/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult EditAllocations(PermissionAllocationsModel model)
        {
            if (!Authorized(RoleType.SystemManager))
                return Error(Loc.Dic.error_no_permission);

            Budgets_Baskets permissionFromDB;
            List<Budgets_Allocations> existingPermissionAllocations;
            List<Budgets_BasketsToAllocation> existingPermissionToAllocations;

            using (BudgetsRepository budgetsRep = new BudgetsRepository(CurrentUser.CompanyId))
            using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
            using (BasketsToAllocationsRepository permissionsAllocationsRep = new BasketsToAllocationsRepository())
            using (AllocationRepository allocationsRep = new AllocationRepository(CurrentUser.CompanyId))
            {
                permissionFromDB = permissionsRep.GetEntity(model.Basket.Id);
                //TODO: Error gets ALL pemissions from DB
                existingPermissionAllocations = permissionsAllocationsRep.GetList().Where(x => x.BasketId == permissionFromDB.Id).Select(y => y.Budgets_Allocations).ToList();
                existingPermissionToAllocations = permissionsAllocationsRep.GetList().Where(x => x.BasketId == permissionFromDB.Id).ToList();

                if (permissionFromDB == null)
                    return Error(Loc.Dic.error_database_error);

                if (permissionFromDB.CompanyId != CurrentUser.CompanyId)
                    return Error(Loc.Dic.error_no_permission);

                Budget budgetFromDB = budgetsRep.GetEntity(model.BudgetAllocations.Budget.Id);

                if (budgetFromDB == null)
                    return Error(Loc.Dic.error_database_error);

                if (budgetFromDB.CompanyId != CurrentUser.CompanyId)
                    return Error(Loc.Dic.error_no_permission);

                foreach (var allocation in model.BudgetAllocations.PermissionAllocations)
                {
                    if (allocation.IsActive)
                    {
                        if (!existingPermissionAllocations.Any(x => x.Id == allocation.Allocation.BudgetsAllocationId))
                        {
                            allocation.Allocation.BudgetId = budgetFromDB.Id;
                            allocation.Allocation.BasketId = permissionFromDB.Id;
                            if(!permissionsAllocationsRep.Create(allocation.Allocation)) return Error(Loc.Dic.error_database_error);
                        }
                    }
                    else
                    {
                        if (existingPermissionAllocations.Any(x => x.Id == allocation.Allocation.BudgetsAllocationId))
                        {
                            permissionsAllocationsRep.Delete(allocation.Allocation.Id);
                        }
                    }
                }

                return RedirectToAction("BudgetBaskets", new { id = budgetFromDB.Id });
            }
        }

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
                using (BasketsToAllocationsRepository permissionsAllocationsRep = new BasketsToAllocationsRepository())
                using (UsersToBasketsRepository usersPermissionsRep = new UsersToBasketsRepository())
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


        [ChildActionOnly]
        public ActionResult List(IEnumerable<Budgets_Baskets> baskets, string baseUrl, bool isOrdered, bool isPaged, string sortby, string order, int currPage, int numberOfPages, bool isCheckBoxed = false, bool showAllocations = false, int? budgetId = null)
        {
            ViewBag.BaseUrl = baseUrl;
            ViewBag.IsOrdered = isOrdered;
            ViewBag.IsPaged = isPaged;
            ViewBag.Sortby = sortby;
            ViewBag.Order = order;
            ViewBag.CurrPage = currPage;
            ViewBag.NumberOfPages = numberOfPages;

            ViewBag.IsCheckBoxed = isCheckBoxed;

            ViewBag.UserRoles = CurrentUser.Roles;

            ViewBag.ShowAllocations = showAllocations;
            ViewBag.budgetId = budgetId;
            return PartialView(baskets);
        }

        private IEnumerable<Budgets_Baskets> Pagination(IEnumerable<Budgets_Baskets> baskets, int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER, bool showAllResults = false)
        {
            int numberOfItems = baskets.Count();
            int numberOfPages = numberOfItems / ITEMS_PER_PAGE;
            if (numberOfItems % ITEMS_PER_PAGE != 0)
                numberOfPages++;

            if (page <= 0)
                page = FIRST_PAGE;
            if (page > numberOfPages)
                page = numberOfPages;

            if (sortby != NO_SORT_BY)
            {
                Func<Func<Budgets_Baskets, dynamic>, IEnumerable<Budgets_Baskets>> orderFunction;

                if (order == DEFAULT_DESC_ORDER)
                    orderFunction = x => baskets.OrderByDescending(x);
                else
                    orderFunction = x => baskets.OrderBy(x);

                switch (sortby)
                {
                    case "name":
                    default:
                        baskets = orderFunction(x => x.Name);
                        break;
                }
            }

            if (!showAllResults)
            {
                baskets = baskets
                    .Skip((page - 1) * ITEMS_PER_PAGE)
                    .Take(ITEMS_PER_PAGE)
                    .ToList();
            }

            ViewBag.Sortby = sortby;
            ViewBag.Order = order;
            ViewBag.CurrPage = page;
            ViewBag.NumberOfPages = numberOfPages;

            return baskets;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}